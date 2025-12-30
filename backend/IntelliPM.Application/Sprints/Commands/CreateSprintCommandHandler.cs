using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Application.Sprints.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Sprints.Commands;

public class CreateSprintCommandHandler : IRequestHandler<CreateSprintCommand, SprintDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CreateSprintCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<SprintDto> Handle(CreateSprintCommand request, CancellationToken cancellationToken)
    {
        // Permission check
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(request.ProjectId, request.CurrentUserId), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanManageSprints(userRole.Value))
            throw new UnauthorizedException("You don't have permission to create sprints in this project");

        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Sprint name is required");

        if (request.EndDate <= request.StartDate)
            throw new ArgumentException("End date must be after start date");

        if (request.Capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0");

        // Verify project exists
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.GetByIdAsync(request.ProjectId, cancellationToken);
        
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.ProjectId} not found");

        // Calculate next sprint number for this project
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var maxSprintNumber = await sprintRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId)
            .Select(s => (int?)s.Number)
            .MaxAsync(cancellationToken);

        var nextNumber = (maxSprintNumber ?? 0) + 1;

        // Create sprint
        var sprint = new Sprint
        {
            ProjectId = request.ProjectId,
            Number = nextNumber,
            Goal = request.Goal ?? string.Empty,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Planned",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await sprintRepo.AddAsync(sprint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern
        var sprintCreatedEvent = new SprintCreatedEvent
        {
            SprintId = sprint.Id,
            ProjectId = sprint.ProjectId,
            OrganizationId = project.OrganizationId,
            Number = sprint.Number,
            Goal = sprint.Goal,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            Status = sprint.Status
        };

        var eventType = typeof(SprintCreatedEvent).AssemblyQualifiedName ?? typeof(SprintCreatedEvent).FullName ?? "SprintCreatedEvent";
        var eventPayload = JsonSerializer.Serialize(sprintCreatedEvent);
        var idempotencyKey = $"sprint-created-{sprint.Id}-{DateTimeOffset.UtcNow.Ticks}";
        var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return DTO
        return new SprintDto(
            sprint.Id,
            sprint.ProjectId,
            project.Name,
            sprint.Number,
            sprint.Goal,
            sprint.StartDate,
            sprint.EndDate,
            sprint.Status,
            0, // No items yet
            sprint.CreatedAt
        );
    }
}
