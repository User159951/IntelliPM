using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Projects.Commands;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public RemoveMemberCommandHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Unit> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        // Get project and verify it exists
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.Query()
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");

        // Get current user's role in the project
        var currentUserMember = project.Members.FirstOrDefault(m => m.UserId == request.CurrentUserId);
        if (currentUserMember == null)
            throw new UnauthorizedAccessException($"User {request.CurrentUserId} is not a member of project {request.ProjectId}");

        // Check if current user has permission to remove members
        if (!ProjectPermissions.CanRemoveMembers(currentUserMember.Role))
            throw new UnauthorizedAccessException($"User {request.CurrentUserId} does not have permission to remove members from this project");

        // Find the member to remove
        var memberToRemove = project.Members.FirstOrDefault(m => m.UserId == request.UserId);
        if (memberToRemove == null)
            throw new NotFoundException($"User {request.UserId} is not a member of project {request.ProjectId}");

        // Prevent removing the ProductOwner
        if (memberToRemove.Role == ProjectRole.ProductOwner)
            throw new InvalidOperationException("Cannot remove ProductOwner from project. Transfer ownership first.");

        // Get user info for activity log
        var userRepo = _unitOfWork.Repository<User>();
        var removedUser = await userRepo.GetByIdAsync(request.UserId, cancellationToken);

        // Remove the member
        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        memberRepo.Delete(memberToRemove);

        // Create activity log
        var activityRepo = _unitOfWork.Repository<Domain.Entities.Activity>();
        await activityRepo.AddAsync(new Domain.Entities.Activity
        {
            UserId = request.CurrentUserId,
            ActivityType = "member_removed",
            EntityType = "project_member",
            EntityId = memberToRemove.Id,
            EntityName = removedUser != null ? $"{removedUser.FirstName} {removedUser.LastName}" : $"User {request.UserId}",
            ProjectId = request.ProjectId,
            ProjectName = project.Name,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern
        var memberRemovedEvent = new MemberRemovedFromProjectEvent
        {
            ProjectId = request.ProjectId,
            OrganizationId = project.OrganizationId,
            UserId = request.UserId
        };

        var eventType = typeof(MemberRemovedFromProjectEvent).AssemblyQualifiedName ?? typeof(MemberRemovedFromProjectEvent).FullName ?? "MemberRemovedFromProjectEvent";
        var eventPayload = JsonSerializer.Serialize(memberRemovedEvent);
        var idempotencyKey = $"member-removed-{request.ProjectId}-{request.UserId}-{DateTimeOffset.UtcNow.Ticks}";
        var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

