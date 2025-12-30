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

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMediator _mediator;

    public CreateTaskCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Permission check
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(request.ProjectId, request.CreatedById), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanCreateTasks(userRole.Value))
            throw new UnauthorizedException("You don't have permission to create tasks in this project");

        // Validation
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");

        if (request.Title.Length > IntelliPM.Domain.Constants.TaskConstants.Validation.TitleMaxLength)
            throw new ArgumentException($"Title must not exceed {IntelliPM.Domain.Constants.TaskConstants.Validation.TitleMaxLength} characters");

        if (!IntelliPM.Domain.Constants.TaskConstants.Priorities.All.Contains(request.Priority))
            throw new ArgumentException($"Priority must be one of: {string.Join(", ", IntelliPM.Domain.Constants.TaskConstants.Priorities.All)}");

        // Verify project exists
        var projectRepo = _unitOfWork.Repository<Project>();
        var projectExists = await projectRepo.Query()
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        
        if (!projectExists)
            throw new InvalidOperationException($"Project with ID {request.ProjectId} not found");

        // Verify assignee exists if provided
        if (request.AssigneeId.HasValue)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var assigneeExists = await userRepo.Query()
                .AsNoTracking()
                .AnyAsync(u => u.Id == request.AssigneeId.Value, cancellationToken);
            
            if (!assigneeExists)
                throw new InvalidOperationException($"User with ID {request.AssigneeId.Value} not found");
        }

        // Create task
        var task = new ProjectTask
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            Status = IntelliPM.Domain.Constants.TaskConstants.Statuses.Todo,
            Priority = request.Priority,
            StoryPoints = request.StoryPoints.HasValue ? new StoryPoints(request.StoryPoints.Value) : null,
            AssigneeId = request.AssigneeId,
            CreatedById = request.CreatedById,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        await taskRepo.AddAsync(task, cancellationToken);

        // Create activity log
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        var project = await projectRepo.GetByIdAsync(request.ProjectId, cancellationToken);
        await activityRepo.AddAsync(new IntelliPM.Domain.Entities.Activity
        {
            UserId = request.CreatedById,
            ActivityType = "task_created",
            EntityType = "task",
            EntityId = task.Id,
            EntityName = task.Title,
            ProjectId = request.ProjectId,
            ProjectName = project?.Name,
            CreatedAt = DateTimeOffset.UtcNow,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern for reliable event processing
        var taskCreatedEvent = new TaskCreatedEvent
        {
            TaskId = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Status = task.Status,
            Priority = task.Priority,
            StoryPoints = task.StoryPoints?.Value,
            SprintId = task.SprintId
        };

        var eventType = typeof(TaskCreatedEvent).AssemblyQualifiedName ?? typeof(TaskCreatedEvent).FullName ?? "TaskCreatedEvent";
        var eventPayload = JsonSerializer.Serialize(taskCreatedEvent);
        var idempotencyKey = $"task-created-{task.Id}-{DateTimeOffset.UtcNow.Ticks}";
        var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveByPrefixAsync($"project-tasks:{request.ProjectId}", cancellationToken);
        await _cache.RemoveByPrefixAsync($"dashboard-metrics:", cancellationToken);

        // Reload task with navigation properties to return TaskDto (Performance: Load only once with all includes)
        var createdTask = await taskRepo.Query()
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Sprint)
            .Include(t => t.CreatedBy)
            .Include(t => t.UpdatedBy)
            .FirstAsync(t => t.Id == task.Id, cancellationToken);

        return new TaskDto(
            createdTask.Id,
            createdTask.ProjectId,
            createdTask.Project.Name,
            createdTask.Title,
            createdTask.Description,
            createdTask.Status,
            createdTask.Priority,
            createdTask.StoryPoints?.Value,
            createdTask.AssigneeId,
            createdTask.Assignee?.Username,
            createdTask.SprintId,
            createdTask.Sprint != null ? $"Sprint {createdTask.Sprint.Number}" : null,
            createdTask.CreatedById,
            createdTask.CreatedBy.Username,
            createdTask.UpdatedById,
            createdTask.UpdatedBy?.Username,
            createdTask.CreatedAt,
            createdTask.UpdatedAt
        );
    }
}
