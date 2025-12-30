using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Sprints.Commands;

public class CompleteSprintCommandHandler : IRequestHandler<CompleteSprintCommand, CompleteSprintResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CompleteSprintCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<CompleteSprintResponse> Handle(CompleteSprintCommand request, CancellationToken cancellationToken)
    {
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        
        // Load sprint
        var sprint = await sprintRepo.Query()
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken);

        if (sprint == null)
            throw new InvalidOperationException($"Sprint with ID {request.SprintId} not found");

        // Permission check
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(sprint.ProjectId, request.UpdatedBy), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanManageSprints(userRole.Value))
            throw new UnauthorizedException("You don't have permission to complete sprints in this project");

        // Check current status
        if (sprint.Status == "Completed")
            throw new InvalidOperationException($"Sprint {request.SprintId} is already completed");

        if (sprint.Status == "Planned")
            throw new InvalidOperationException($"Cannot complete a sprint that hasn't been started");

        // Get task statistics
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var sprintTasks = await taskRepo.Query()
            .Where(t => t.SprintId == request.SprintId)
            .ToListAsync(cancellationToken);

        var totalTasksCount = sprintTasks.Count;
        var completedTasksCount = sprintTasks.Count(t => t.Status == "Done" || t.Status == "Completed");
        var incompleteTasks = sprintTasks.Where(t => t.Status != "Done" && t.Status != "Completed").ToList();
        var totalStoryPoints = sprintTasks.Sum(t => t.StoryPoints?.Value ?? 0);
        var completedStoryPoints = sprintTasks.Where(t => t.Status == "Done" || t.Status == "Completed")
            .Sum(t => t.StoryPoints?.Value ?? 0);

        // Handle incomplete tasks based on action
        if (incompleteTasks.Any() && !string.IsNullOrEmpty(request.IncompleteTasksAction))
        {
            switch (request.IncompleteTasksAction.ToLower())
            {
                case "next_sprint":
                    // Move to next planned sprint (if exists)
                    var nextSprint = await sprintRepo.Query()
                        .Where(s => s.ProjectId == sprint.ProjectId && s.Status == "Planned" && s.Id != sprint.Id)
                        .OrderBy(s => s.StartDate ?? s.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);
                    
                    if (nextSprint != null)
                    {
                        foreach (var task in incompleteTasks)
                        {
                            task.SprintId = nextSprint.Id;
                            task.UpdatedAt = DateTimeOffset.UtcNow;
                            task.UpdatedById = request.UpdatedBy;
                            taskRepo.Update(task);
                        }
                    }
                    else
                    {
                        // No next sprint, move to backlog
                        foreach (var task in incompleteTasks)
                        {
                            task.SprintId = null;
                            task.UpdatedAt = DateTimeOffset.UtcNow;
                            task.UpdatedById = request.UpdatedBy;
                            taskRepo.Update(task);
                        }
                    }
                    break;

                case "backlog":
                    // Move to backlog (remove sprint assignment)
                    foreach (var task in incompleteTasks)
                    {
                        task.SprintId = null;
                        task.UpdatedAt = DateTimeOffset.UtcNow;
                        task.UpdatedById = request.UpdatedBy;
                        taskRepo.Update(task);
                    }
                    break;

                case "keep":
                    // Keep in current sprint (no action needed)
                    break;
            }
        }
        else if (incompleteTasks.Any())
        {
            // Default: move to backlog if no action specified
            foreach (var task in incompleteTasks)
            {
                task.SprintId = null;
                task.UpdatedAt = DateTimeOffset.UtcNow;
                task.UpdatedById = request.UpdatedBy;
                taskRepo.Update(task);
            }
        }

        // Complete sprint
        sprint.Status = "Completed";
        
        // Update EndDate if not set or if it's in the past
        if (!sprint.EndDate.HasValue || sprint.EndDate.Value < DateTimeOffset.UtcNow)
        {
            sprint.EndDate = DateTimeOffset.UtcNow;
        }

        sprintRepo.Update(sprint);

        // Create activity log
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        await activityRepo.AddAsync(new IntelliPM.Domain.Entities.Activity
        {
            UserId = request.UpdatedBy,
            ActivityType = "sprint_completed",
            EntityType = "sprint",
            EntityId = sprint.Id,
            EntityName = $"Sprint {sprint.Number}",
            ProjectId = sprint.ProjectId,
            ProjectName = sprint.Project.Name,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { completedTasks = completedTasksCount, totalTasks = totalTasksCount }),
            CreatedAt = DateTimeOffset.UtcNow,
        }, cancellationToken);

        // Create alert for sprint completion
        var alertRepo = _unitOfWork.Repository<Alert>();
        await alertRepo.AddAsync(new Alert
        {
            ProjectId = sprint.ProjectId,
            Type = "SprintCompleted",
            Severity = "Info",
            Title = "Sprint completed",
            Message = $"Sprint {sprint.Number} for project '{sprint.Project.Name}' is completed. {completedTasksCount}/{totalTasksCount} tasks done.",
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern
        var sprintCompletedEvent = new SprintCompletedEvent
        {
            SprintId = sprint.Id,
            ProjectId = sprint.ProjectId,
            OrganizationId = sprint.OrganizationId,
            CompletedStoryPoints = completedStoryPoints
        };

        var eventType = typeof(SprintCompletedEvent).AssemblyQualifiedName ?? typeof(SprintCompletedEvent).FullName ?? "SprintCompletedEvent";
        var eventPayload = JsonSerializer.Serialize(sprintCompletedEvent);
        var idempotencyKey = $"sprint-completed-{sprint.Id}-{DateTimeOffset.UtcNow.Ticks}";
        var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Calculate velocity and completion rate
        var velocity = completedStoryPoints;
        var completionRate = totalTasksCount > 0 ? (decimal)completedTasksCount / totalTasksCount * 100 : 0;

        return new CompleteSprintResponse(
            sprint.Id,
            sprint.Status,
            sprint.EndDate.Value,
            DateTimeOffset.UtcNow,
            completedTasksCount,
            totalTasksCount,
            velocity,
            completionRate
        );
    }
}

