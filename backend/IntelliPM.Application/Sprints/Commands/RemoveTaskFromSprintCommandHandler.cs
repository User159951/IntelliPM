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
/// Handler for removing tasks from a sprint and returning them to the backlog.
/// </summary>
public class RemoveTaskFromSprintCommandHandler : IRequestHandler<RemoveTaskFromSprintCommand, RemoveTaskFromSprintResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<RemoveTaskFromSprintCommandHandler> _logger;

    private const int DefaultSprintCapacity = 40; // Default story points capacity
    private const int StoryPointsPerMember = 20; // Story points per team member for capacity calculation

    public RemoveTaskFromSprintCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<RemoveTaskFromSprintCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RemoveTaskFromSprintResponse> Handle(RemoveTaskFromSprintCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedException("You don't have permission to manage sprints in this project. Only Product Owners and Scrum Masters can remove tasks from sprints.");
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

        // 4. Process each task to remove
        var removedTasks = new List<TaskRemovedDto>();
        var currentUserIdValue = currentUserId; // Capture for closure
        var tasksToRemove = new List<ProjectTask>();

        foreach (var task in tasks)
        {
            // Verify task belongs to same project as sprint
            if (task.ProjectId != sprint.ProjectId)
            {
                _logger.LogWarning(
                    "Task {TaskId} belongs to project {TaskProjectId}, but sprint belongs to project {SprintProjectId}",
                    task.Id,
                    task.ProjectId,
                    sprint.ProjectId);
                continue; // Skip this task but continue with others
            }

            // Check if task.SprintId matches SprintId (skip if not in this sprint)
            var wasInSprint = task.SprintId.HasValue && task.SprintId.Value == sprint.Id;

            if (!wasInSprint)
            {
                _logger.LogDebug(
                    "Task {TaskId} is not assigned to sprint {SprintId}. Current SprintId: {CurrentSprintId}",
                    task.Id,
                    sprint.Id,
                    task.SprintId);
                // Still track it in the response but mark as not being in sprint
            }
            else
            {
                // Set task.SprintId = null (remove from sprint)
                task.SprintId = null;
                task.UpdatedAt = DateTimeOffset.UtcNow;
                task.UpdatedById = currentUserIdValue;
                taskRepo.Update(task);
                tasksToRemove.Add(task);
            }

            // Track in RemovedTasks list with WasInSprint flag
            removedTasks.Add(new TaskRemovedDto(
                task.Id,
                task.Title,
                task.StoryPoints?.Value,
                wasInSprint
            ));
        }

        if (tasksToRemove.Count == 0 && removedTasks.All(t => !t.WasInSprint))
        {
            _logger.LogWarning(
                "No tasks were actually in sprint {SprintId} to remove",
                sprint.Id);
            // Continue anyway to return the updated capacity
        }

        // Save changes via UnitOfWork
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Recalculate sprint capacity after removal
        // Sum remaining story points of tasks still in sprint (where SprintId == sprint.Id and Status != "Done")
        var remainingTasksInSprint = await taskRepo.Query()
            .Where(t => t.SprintId == sprint.Id && t.Status != TaskConstants.Statuses.Done)
            .ToListAsync(cancellationToken);

        var remainingStoryPoints = remainingTasksInSprint
            .Where(t => t.StoryPoints != null)
            .Sum(t => t.StoryPoints!.Value);

        // Calculate planned capacity
        var plannedCapacity = CalculateSprintCapacity(sprint.Project.Members.Count);

        // Calculate remaining capacity and utilization
        var remainingCapacity = plannedCapacity - remainingStoryPoints;
        var capacityUtilization = plannedCapacity > 0
            ? Math.Round((decimal)remainingStoryPoints / plannedCapacity * 100, 2)
            : 0;

        var freedStoryPoints = tasksToRemove
            .Where(t => t.StoryPoints != null)
            .Sum(t => t.StoryPoints!.Value);

        _logger.LogInformation(
            "Removed {TaskCount} task(s) from sprint {SprintId} (Sprint: {SprintName}). Freed {FreedPoints} story points. Remaining: {RemainingPoints}/{Capacity}. Utilization: {Utilization}%",
            tasksToRemove.Count,
            sprint.Id,
            $"Sprint {sprint.Number}",
            freedStoryPoints,
            remainingStoryPoints,
            plannedCapacity,
            capacityUtilization);

        // Return RemoveTaskFromSprintResponse with updated capacity
        return new RemoveTaskFromSprintResponse(
            sprint.Id,
            $"Sprint {sprint.Number}",
            removedTasks,
            new SprintCapacityDto(
                remainingStoryPoints,
                plannedCapacity,
                Math.Max(0, remainingCapacity),
                capacityUtilization
            )
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

