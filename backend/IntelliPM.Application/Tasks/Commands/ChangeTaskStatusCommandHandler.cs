using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Application.Services;
using System.Text.Json;

namespace IntelliPM.Application.Tasks.Commands;

public class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand, ChangeTaskStatusResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMediator _mediator;
    private readonly WorkflowTransitionValidator _workflowValidator;

    public ChangeTaskStatusCommandHandler(
        IUnitOfWork unitOfWork, 
        ICacheService cache, 
        IMediator mediator,
        WorkflowTransitionValidator workflowValidator)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mediator = mediator;
        _workflowValidator = workflowValidator;
    }

    public async Task<ChangeTaskStatusResponse> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var task = await taskRepo.GetByIdAsync(request.TaskId, cancellationToken);
        
        if (task == null)
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");

        // Permission check
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(task.ProjectId, request.UpdatedBy), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanEditTasks(userRole.Value))
            throw new UnauthorizedException("You don't have permission to change task status in this project");

        // Validate status (Todo, InProgress, InReview, Done, Blocked)
        var validStatuses = new[] { "Todo", "InProgress", "InReview", "Done", "Blocked" };
        if (!validStatuses.Contains(request.NewStatus))
            throw new ArgumentException($"Invalid status: {request.NewStatus}. Must be one of: {string.Join(", ", validStatuses)}");

        var oldStatus = task.Status;

        // Validate workflow transition
        var validationResult = await _workflowValidator.ValidateTransitionAsync(
            entityType: "Task",
            fromStatus: oldStatus,
            toStatus: request.NewStatus,
            userRole: userRole.Value,
            entityId: task.Id,
            userId: request.UpdatedBy,
            projectId: task.ProjectId,
            checkConditions: null, // Can be extended to check conditions like "QAApproval" for Done->Released
            cancellationToken: cancellationToken);

        if (!validationResult.IsAllowed)
        {
            throw new UnauthorizedException($"Workflow transition not allowed: {validationResult.Reason}");
        }
        task.Status = request.NewStatus;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        task.UpdatedById = request.UpdatedBy;

        taskRepo.Update(task);

        // Create activity log
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.GetByIdAsync(task.ProjectId, cancellationToken);
        var userRepo = _unitOfWork.Repository<User>();
        
        var activityType = request.NewStatus == "Done" ? "task_completed" : "task_status_changed";
        await activityRepo.AddAsync(new IntelliPM.Domain.Entities.Activity
        {
            UserId = request.UpdatedBy,
            ActivityType = activityType,
            EntityType = "task",
            EntityId = task.Id,
            EntityName = task.Title,
            ProjectId = task.ProjectId,
            ProjectName = project?.Name,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { oldStatus, newStatus = request.NewStatus }),
            CreatedAt = DateTimeOffset.UtcNow,
        }, cancellationToken);

        // Create notification when task is completed (notify project owner and assignee)
        if (request.NewStatus == "Done")
        {
            var completer = await userRepo.GetByIdAsync(request.UpdatedBy, cancellationToken);
            var organizationId = project?.OrganizationId ?? task.OrganizationId;
            
            var notificationRepo = _unitOfWork.Repository<Notification>();
            
            // Notify assignee if different from completer
            if (task.AssigneeId.HasValue && task.AssigneeId.Value != request.UpdatedBy)
            {
                await notificationRepo.AddAsync(new Notification
                {
                    UserId = task.AssigneeId.Value,
                    OrganizationId = organizationId,
                    Type = "task_completed",
                    Message = $"{completer?.FirstName} {completer?.LastName} completed '{task.Title}'",
                    EntityType = "task",
                    EntityId = task.Id,
                    ProjectId = task.ProjectId,
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
            
            // Notify project owner if different from completer and assignee
            if (project != null && project.OwnerId != request.UpdatedBy && 
                (!task.AssigneeId.HasValue || project.OwnerId != task.AssigneeId.Value))
            {
                await notificationRepo.AddAsync(new Notification
                {
                    UserId = project.OwnerId,
                    OrganizationId = organizationId,
                    Type = "task_completed",
                    Message = $"{completer?.FirstName} {completer?.LastName} completed '{task.Title}'",
                    EntityType = "task",
                    EntityId = task.Id,
                    ProjectId = task.ProjectId,
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern for reliable event processing
        var taskUpdatedEvent = new TaskUpdatedEvent
        {
            TaskId = task.Id,
            ProjectId = task.ProjectId,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus,
            Changes = new Dictionary<string, string> { { "Status", $"{oldStatus} -> {request.NewStatus}" } }
        };

        var eventType = typeof(TaskUpdatedEvent).AssemblyQualifiedName ?? typeof(TaskUpdatedEvent).FullName ?? "TaskUpdatedEvent";
        var eventPayload = JsonSerializer.Serialize(taskUpdatedEvent);
        var idempotencyKey = $"task-status-changed-{task.Id}-{DateTimeOffset.UtcNow.Ticks}";
        var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache (status changes affect metrics)
        await _cache.RemoveByPrefixAsync($"project-tasks:{task.ProjectId}", cancellationToken);
        await _cache.RemoveByPrefixAsync($"dashboard-metrics:", cancellationToken);

        return new ChangeTaskStatusResponse(
            task.Id,
            task.Status,
            task.UpdatedAt
        );
    }
}

