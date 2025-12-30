using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Sprints.Commands;

/// <summary>
/// Handler for adding tasks to a sprint with automatic velocity calculation.
/// </summary>
public class AddTaskToSprintCommandHandler : IRequestHandler<AddTaskToSprintCommand, AddTaskToSprintResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<AddTaskToSprintCommandHandler> _logger;

    private const int DefaultSprintCapacity = 40; // Default story points capacity
    private const int StoryPointsPerMember = 20; // Story points per team member for capacity calculation

    public AddTaskToSprintCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<AddTaskToSprintCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AddTaskToSprintResponse> Handle(AddTaskToSprintCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        // 1. Fetch sprint by SprintId (throw NotFoundException if not found)
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var sprint = await sprintRepo.Query()
            .Include(s => s.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(s => s.Id == request.SprintId && s.OrganizationId == organizationId, cancellationToken);

        if (sprint == null)
        {
            _logger.LogWarning("Sprint {SprintId} not found in organization {OrganizationId}", request.SprintId, organizationId);
            throw new NotFoundException($"Sprint with ID {request.SprintId} not found");
        }

        // 2. Check if user has permission to manage sprints (ProductOwner or ScrumMaster)
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(sprint.ProjectId, currentUserId), cancellationToken);
        if (userRole == null)
        {
            _logger.LogWarning(
                "User {UserId} is not a member of project {ProjectId}",
                currentUserId,
                sprint.ProjectId);
            throw new UnauthorizedException("You are not a member of this project");
        }

        if (!ProjectPermissions.CanManageSprints(userRole.Value))
        {
            _logger.LogWarning(
                "User {UserId} with role {Role} does not have permission to manage sprints in project {ProjectId}",
                currentUserId,
                userRole.Value,
                sprint.ProjectId);
            throw new UnauthorizedException("You don't have permission to manage sprints in this project. Only Product Owners and Scrum Masters can add tasks to sprints.");
        }

        // Verify sprint belongs to user's organization (already done in query filter, but double-check)
        if (sprint.OrganizationId != organizationId)
        {
            _logger.LogWarning(
                "Sprint {SprintId} belongs to organization {SprintOrgId}, but user belongs to {UserOrgId}",
                sprint.Id,
                sprint.OrganizationId,
                organizationId);
            throw new UnauthorizedException("You don't have access to this sprint");
        }

        // 3. Fetch all tasks by TaskIds
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var tasks = await taskRepo.Query()
            .Where(t => request.TaskIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        if (tasks.Count == 0)
        {
            _logger.LogWarning("No tasks found with provided TaskIds");
            throw new NotFoundException("No tasks found with the provided task IDs");
        }

        if (tasks.Count != request.TaskIds.Count)
        {
            var foundTaskIds = tasks.Select(t => t.Id).ToList();
            var missingTaskIds = request.TaskIds.Except(foundTaskIds).ToList();
            _logger.LogWarning("Some tasks were not found. Missing TaskIds: {TaskIds}", string.Join(", ", missingTaskIds));
            throw new NotFoundException($"Some tasks were not found. Missing task IDs: {string.Join(", ", missingTaskIds)}");
        }

        // 4. Validate all tasks belong to same project as sprint
        var tasksInDifferentProject = tasks.Where(t => t.ProjectId != sprint.ProjectId).ToList();
        if (tasksInDifferentProject.Any())
        {
            var invalidTaskIds = tasksInDifferentProject.Select(t => t.Id).ToList();
            _logger.LogWarning(
                "Tasks {TaskIds} belong to different project than sprint {SprintId}",
                string.Join(", ", invalidTaskIds),
                sprint.Id);
            throw new ValidationException($"Tasks {string.Join(", ", invalidTaskIds)} belong to a different project than the sprint");
        }

        // 5. Calculate current sprint capacity
        // Sum all story points of tasks already in sprint (where SprintId == sprint.Id and Status != "Done")
        var existingTasksInSprint = await taskRepo.Query()
            .Where(t => t.SprintId == sprint.Id && t.Status != TaskConstants.Statuses.Done)
            .ToListAsync(cancellationToken);

        var currentStoryPoints = existingTasksInSprint
            .Where(t => t.StoryPoints != null)
            .Sum(t => t.StoryPoints!.Value);

        // Get sprint's planned capacity
        // Since Sprint entity doesn't store PlannedCapacity, calculate based on team members or use default
        var plannedCapacity = CalculateSprintCapacity(sprint.Project.Members.Count);

        // 6. Process each task to add
        var addedTasks = new List<TaskAddedDto>();
        var newStoryPointsTotal = currentStoryPoints;
        var currentUserIdValue = currentUserId; // Capture for closure

        foreach (var task in tasks)
        {
            // Check if task is already in sprint
            var wasAlreadyInSprint = task.SprintId.HasValue && task.SprintId.Value == sprint.Id;

            if (!wasAlreadyInSprint)
            {
                // Add task's story points to total (before assignment)
                if (task.StoryPoints != null)
                {
                    newStoryPointsTotal += task.StoryPoints.Value;
                }

                // Set task.SprintId = SprintId
                task.SprintId = sprint.Id;
                task.UpdatedAt = DateTimeOffset.UtcNow;
                task.UpdatedById = currentUserIdValue;
                taskRepo.Update(task);
            }

            // Track in AddedTasks list
            addedTasks.Add(new TaskAddedDto(
                task.Id,
                task.Title,
                task.StoryPoints?.Value,
                wasAlreadyInSprint
            ));
        }

        // 7. Calculate remaining capacity and utilization percentage
        var remainingCapacity = plannedCapacity - newStoryPointsTotal;
        var capacityUtilization = plannedCapacity > 0
            ? Math.Round((decimal)newStoryPointsTotal / plannedCapacity * 100, 2)
            : 0;

        // 8. Check if over capacity
        var isOverCapacity = newStoryPointsTotal > plannedCapacity;
        string? capacityWarning = null;

        if (isOverCapacity && !request.IgnoreCapacityWarning)
        {
            var overCapacity = newStoryPointsTotal - plannedCapacity;
            capacityWarning = $"Sprint is over capacity by {overCapacity} story points (Total: {newStoryPointsTotal}, Capacity: {plannedCapacity})";
            _logger.LogWarning(
                "Sprint {SprintId} is over capacity by {OverCapacity} story points. Total: {Total}, Capacity: {Capacity}",
                sprint.Id,
                overCapacity,
                newStoryPointsTotal,
                plannedCapacity);
        }

        // 9. Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Added {TaskCount} task(s) to sprint {SprintId} (Sprint: {SprintName}). Total story points: {TotalPoints}/{Capacity}. Over capacity: {IsOverCapacity}",
            addedTasks.Count,
            sprint.Id,
            $"Sprint {sprint.Number}",
            newStoryPointsTotal,
            plannedCapacity,
            isOverCapacity);

        // 10. Return AddTaskToSprintResponse
        return new AddTaskToSprintResponse(
            sprint.Id,
            $"Sprint {sprint.Number}",
            addedTasks,
            new SprintCapacityDto(
                newStoryPointsTotal,
                plannedCapacity,
                Math.Max(0, remainingCapacity),
                capacityUtilization
            ),
            isOverCapacity,
            capacityWarning
        );
    }

    /// <summary>
    /// Calculates sprint capacity based on team size or uses default.
    /// </summary>
    private int CalculateSprintCapacity(int teamMemberCount)
    {
        // Calculate capacity: team members * story points per member, or default if no members
        if (teamMemberCount > 0)
        {
            return teamMemberCount * StoryPointsPerMember;
        }

        return DefaultSprintCapacity;
    }
}

