using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Projects.Commands;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, UpdateProjectResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMediator _mediator;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task<UpdateProjectResponse> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        // Permission check
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(request.ProjectId, request.CurrentUserId), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanEditProject(userRole.Value))
            throw new UnauthorizedException("You don't have permission to edit this project");

        var projectRepo = _unitOfWork.Repository<Project>();
        
        // Load existing project with members for cache invalidation
        var project = await projectRepo.Query()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.ProjectId} not found");

        // Track changes for event
        var changes = new Dictionary<string, string>();

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.Name) && project.Name != request.Name)
        {
            changes["Name"] = $"{project.Name} -> {request.Name}";
            project.Name = request.Name;
        }

        if (request.Description != null && project.Description != request.Description)
        {
            changes["Description"] = "Updated";
            project.Description = request.Description;
        }

        var oldStatus = project.Status;
        if (!string.IsNullOrWhiteSpace(request.Status) && project.Status != request.Status)
        {
            // Validate status
            if (request.Status != "Active" && request.Status != "Archived")
                throw new ArgumentException("Status must be 'Active' or 'Archived'");
            
            changes["Status"] = $"{project.Status} -> {request.Status}";
            project.Status = request.Status;
        }

        if (!string.IsNullOrWhiteSpace(request.Type) && project.Type != request.Type)
        {
            // Validate type
            if (request.Type != "Scrum" && request.Type != "Kanban")
                throw new ArgumentException("Type must be 'Scrum' or 'Kanban'");
            
            changes["Type"] = $"{project.Type} -> {request.Type}";
            project.Type = request.Type;
        }

        if (request.SprintDurationDays.HasValue && project.SprintDurationDays != request.SprintDurationDays.Value)
        {
            if (request.SprintDurationDays.Value <= 0)
                throw new ArgumentException("Sprint duration must be greater than 0");
            
            changes["SprintDurationDays"] = $"{project.SprintDurationDays} -> {request.SprintDurationDays.Value}";
            project.SprintDurationDays = request.SprintDurationDays.Value;
        }

        // Set UpdatedAt timestamp
        project.UpdatedAt = DateTimeOffset.UtcNow;

        // Save changes
        projectRepo.Update(project);

        // Create activity log if status changed to Archived
        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status == "Archived" && oldStatus != "Archived")
        {
            var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
            await activityRepo.AddAsync(new IntelliPM.Domain.Entities.Activity
            {
                UserId = request.CurrentUserId,
                ActivityType = "project_archived",
                EntityType = "project",
                EntityId = project.Id,
                EntityName = project.Name,
                ProjectId = project.Id,
                ProjectName = project.Name,
                CreatedAt = DateTimeOffset.UtcNow,
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern if there are changes
        if (changes.Count > 0)
        {
            var projectUpdatedEvent = new ProjectUpdatedEvent
            {
                ProjectId = project.Id,
                Changes = changes
            };

            var eventType = typeof(ProjectUpdatedEvent).AssemblyQualifiedName ?? typeof(ProjectUpdatedEvent).FullName ?? "ProjectUpdatedEvent";
            var eventPayload = JsonSerializer.Serialize(projectUpdatedEvent);
            var idempotencyKey = $"project-updated-{project.Id}-{DateTimeOffset.UtcNow.Ticks}";
            var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

            var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
            await outboxRepo.AddAsync(outboxMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Invalidate cache for project owner and all members (all paginated entries)
        await _cache.RemoveByPrefixAsync($"user-projects:{project.OwnerId}:", cancellationToken);
        await _cache.RemoveByPrefixAsync($"project-details:{project.Id}", cancellationToken);
        await _cache.RemoveByPrefixAsync($"project-tasks:{project.Id}", cancellationToken);
        foreach (var member in project.Members)
        {
            await _cache.RemoveByPrefixAsync($"user-projects:{member.UserId}:", cancellationToken);
        }

        return new UpdateProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.Type,
            project.Status,
            project.SprintDurationDays,
            project.UpdatedAt
        );
    }
}

