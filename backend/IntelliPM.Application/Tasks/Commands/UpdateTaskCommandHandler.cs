using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Application.Tasks.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using IntelliPM.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Tasks.Commands;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMediator _mediator;

    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        
        // Load existing task
        var task = await taskRepo.GetByIdAsync(request.TaskId, cancellationToken);
        
        if (task == null)
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");

        // Store old values for event
        var oldStatus = task.Status;
        var oldSprintId = task.SprintId;
        var changes = new Dictionary<string, string>();

        // Permission check
        if (request.UpdatedById > 0)
        {
            var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(task.ProjectId, request.UpdatedById), cancellationToken);
            if (userRole == null)
                throw new UnauthorizedException("You are not a member of this project");
            if (!ProjectPermissions.CanEditTasks(userRole.Value))
                throw new UnauthorizedException("You don't have permission to edit tasks in this project");
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            if (request.Title.Length > 200)
                throw new ArgumentException("Title must not exceed 200 characters");
            
            if (task.Title != request.Title)
            {
                changes["Title"] = $"{task.Title} -> {request.Title}";
                task.Title = request.Title;
            }
        }

        if (request.Description != null && task.Description != request.Description)
        {
            changes["Description"] = "Updated";
            task.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            var validPriorities = new[] { "Low", "Medium", "High", "Critical" };
            if (!validPriorities.Contains(request.Priority))
                throw new ArgumentException($"Priority must be one of: {string.Join(", ", validPriorities)}");
            
            if (task.Priority != request.Priority)
            {
                changes["Priority"] = $"{task.Priority} -> {request.Priority}";
                task.Priority = request.Priority;
            }
        }

        if (request.StoryPoints.HasValue)
        {
            var newStoryPoints = request.StoryPoints.Value > 0 
                ? new StoryPoints(request.StoryPoints.Value) 
                : null;
            
            var oldValue = task.StoryPoints?.Value.ToString() ?? "null";
            var newValue = newStoryPoints?.Value.ToString() ?? "null";
            
            if (oldValue != newValue)
            {
                changes["StoryPoints"] = $"{oldValue} -> {newValue}";
                task.StoryPoints = newStoryPoints;
            }
        }

        // Update metadata
        task.UpdatedAt = DateTimeOffset.UtcNow;
        if (request.UpdatedById > 0)
        {
            task.UpdatedById = request.UpdatedById;
        }

        taskRepo.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern for reliable event processing
        if (changes.Count > 0 || oldStatus != task.Status || oldSprintId != task.SprintId)
        {
            var taskUpdatedEvent = new TaskUpdatedEvent
            {
                TaskId = task.Id,
                ProjectId = task.ProjectId,
                OldStatus = oldStatus != task.Status ? oldStatus : null,
                NewStatus = oldStatus != task.Status ? task.Status : null,
                OldSprintId = oldSprintId != task.SprintId ? oldSprintId : null,
                NewSprintId = oldSprintId != task.SprintId ? task.SprintId : null,
                Changes = changes
            };

            var eventType = typeof(TaskUpdatedEvent).AssemblyQualifiedName ?? typeof(TaskUpdatedEvent).FullName ?? "TaskUpdatedEvent";
            var eventPayload = JsonSerializer.Serialize(taskUpdatedEvent);
            var idempotencyKey = $"task-updated-{task.Id}-{DateTimeOffset.UtcNow.Ticks}";
            var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

            var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
            await outboxRepo.AddAsync(outboxMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Invalidate cache
        await _cache.RemoveByPrefixAsync($"project-tasks:{task.ProjectId}", cancellationToken);
        await _cache.RemoveByPrefixAsync($"dashboard-metrics:", cancellationToken);

        // Reload task with navigation properties to return TaskDto
        var updatedTask = await taskRepo.Query()
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Sprint)
            .Include(t => t.CreatedBy)
            .Include(t => t.UpdatedBy)
            .FirstAsync(t => t.Id == task.Id, cancellationToken);

        return new TaskDto(
            updatedTask.Id,
            updatedTask.ProjectId,
            updatedTask.Project.Name,
            updatedTask.Title,
            updatedTask.Description,
            updatedTask.Status,
            updatedTask.Priority,
            updatedTask.StoryPoints?.Value,
            updatedTask.AssigneeId,
            updatedTask.Assignee?.Username,
            updatedTask.SprintId,
            updatedTask.Sprint != null ? $"Sprint {updatedTask.Sprint.Number}" : null,
            updatedTask.CreatedById,
            updatedTask.CreatedBy.Username,
            updatedTask.UpdatedById,
            updatedTask.UpdatedBy?.Username,
            updatedTask.CreatedAt,
            updatedTask.UpdatedAt
        );
    }
}
