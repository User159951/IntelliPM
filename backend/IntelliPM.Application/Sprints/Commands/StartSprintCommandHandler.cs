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

public class StartSprintCommandHandler : IRequestHandler<StartSprintCommand, StartSprintResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public StartSprintCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<StartSprintResponse> Handle(StartSprintCommand request, CancellationToken cancellationToken)
    {
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        
        // Load sprint
        var sprint = await sprintRepo.Query()
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken);

        if (sprint == null)
            throw new InvalidOperationException($"Sprint with ID {request.SprintId} not found");

        // Permission check - EXCLUSIVE to ScrumMaster
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(sprint.ProjectId, request.UpdatedBy), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanStartSprint(userRole.Value))
            throw new UnauthorizedException("Only ScrumMaster can start sprints. Your role: " + userRole.Value);

        // Check current status
        if (sprint.Status == "Active")
            throw new InvalidOperationException($"Sprint {request.SprintId} is already active");

        if (sprint.Status == "Completed")
            throw new InvalidOperationException($"Cannot start a completed sprint");

        // Check if there's already an active sprint for this project
        var activeSprintExists = await sprintRepo.Query()
            .AnyAsync(s => s.ProjectId == sprint.ProjectId && s.Status == "Active" && s.Id != sprint.Id, cancellationToken);

        if (activeSprintExists)
            throw new InvalidOperationException($"Project {sprint.ProjectId} already has an active sprint");

        // Check that sprint has at least 1 task
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var taskCount = await taskRepo.Query()
            .CountAsync(t => t.SprintId == request.SprintId, cancellationToken);

        if (taskCount == 0)
            throw new InvalidOperationException($"Sprint must have at least 1 task before starting");

        // Check that sprint has dates defined
        if (!sprint.StartDate.HasValue && !sprint.EndDate.HasValue)
            throw new InvalidOperationException($"Sprint must have dates defined before starting");

        // Start sprint
        sprint.Status = "Active";
        
        // Update StartDate if not set or if it's in the future
        if (!sprint.StartDate.HasValue || sprint.StartDate.Value > DateTimeOffset.UtcNow)
        {
            sprint.StartDate = DateTimeOffset.UtcNow;
        }

        sprintRepo.Update(sprint);

        // Create activity log
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        await activityRepo.AddAsync(new IntelliPM.Domain.Entities.Activity
        {
            UserId = request.UpdatedBy,
            ActivityType = "sprint_started",
            EntityType = "sprint",
            EntityId = sprint.Id,
            EntityName = $"Sprint {sprint.Number}",
            ProjectId = sprint.ProjectId,
            ProjectName = sprint.Project.Name,
            CreatedAt = DateTimeOffset.UtcNow,
        }, cancellationToken);

        // Create notifications for sprint start (notify all project members)
        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        var projectMembers = await memberRepo.Query()
            .Where(m => m.ProjectId == sprint.ProjectId)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);
        
        var notificationRepo = _unitOfWork.Repository<Notification>();
        var userRepo = _unitOfWork.Repository<User>();
        var starter = await userRepo.GetByIdAsync(request.UpdatedBy, cancellationToken);
        
        foreach (var memberId in projectMembers)
        {
            if (memberId != request.UpdatedBy) // Don't notify the person who started it
            {
                await notificationRepo.AddAsync(new Notification
                {
                    UserId = memberId,
                    Type = "sprint_started",
                    Message = $"{starter?.FirstName} {starter?.LastName} started Sprint {sprint.Number}",
                    EntityType = "sprint",
                    EntityId = sprint.Id,
                    ProjectId = sprint.ProjectId,
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern
        var sprintStartedEvent = new SprintStartedEvent
        {
            SprintId = sprint.Id,
            ProjectId = sprint.ProjectId,
            OrganizationId = sprint.OrganizationId,
            StartDate = sprint.StartDate ?? DateTimeOffset.UtcNow,
            EndDate = sprint.EndDate ?? DateTimeOffset.UtcNow.AddDays(14)
        };

        var eventType = typeof(SprintStartedEvent).AssemblyQualifiedName ?? typeof(SprintStartedEvent).FullName ?? "SprintStartedEvent";
        var eventPayload = JsonSerializer.Serialize(sprintStartedEvent);
        var idempotencyKey = $"sprint-started-{sprint.Id}-{DateTimeOffset.UtcNow.Ticks}";
        var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StartSprintResponse(
            sprint.Id,
            sprint.Status,
            sprint.StartDate ?? DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );
    }
}

