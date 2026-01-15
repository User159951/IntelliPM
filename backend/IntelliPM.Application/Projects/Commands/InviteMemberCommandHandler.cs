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

public class InviteMemberCommandHandler : IRequestHandler<InviteMemberCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public InviteMemberCommandHandler(IUnitOfWork unitOfWork, IAuthService authService, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        // Get project and verify it exists
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.Query()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");

        // Get current user's role in the project
        var currentUserMember = project.Members.FirstOrDefault(m => m.UserId == request.CurrentUserId);
        if (currentUserMember == null)
            throw new UnauthorizedAccessException($"User {request.CurrentUserId} is not a member of project {request.ProjectId}");

        // Check if current user has permission to invite members
        if (!ProjectPermissions.CanInviteMembers(currentUserMember.Role))
            throw new UnauthorizedAccessException($"User {request.CurrentUserId} does not have permission to invite members to this project");

        // Check if user with email exists and belongs to the same organization
        var organizationId = _currentUserService.GetOrganizationId();
        var userRepo = _unitOfWork.Repository<User>();
        var invitedUser = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (invitedUser == null)
            throw new NotFoundException($"User with email {request.Email} not found");
        
        if (invitedUser.OrganizationId != organizationId)
            throw new ValidationException($"User with email {request.Email} does not belong to your organization");

        // Check if user is already a member
        var isAlreadyMember = project.Members.Any(m => m.UserId == invitedUser.Id);
        if (isAlreadyMember)
            throw new InvalidOperationException($"User with email {request.Email} is already a member of this project");

        // Create ProjectMember entry
        var projectMember = new ProjectMember
        {
            ProjectId = request.ProjectId,
            UserId = invitedUser.Id,
            Role = request.Role,
            InvitedById = request.CurrentUserId,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTimeOffset.UtcNow
        };

        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        await memberRepo.AddAsync(projectMember, cancellationToken);

        // Get current user info for notification message
        var currentUser = await userRepo.GetByIdAsync(request.CurrentUserId, cancellationToken);

        // Create notification for the invited user
        var notificationRepo = _unitOfWork.Repository<Notification>();
        await notificationRepo.AddAsync(new Notification
        {
            UserId = invitedUser.Id,
            OrganizationId = organizationId,
            Type = "project_invite",
            Message = $"{currentUser?.FirstName} {currentUser?.LastName} invited you to join project '{project.Name}'",
            EntityType = "project",
            EntityId = project.Id,
            ProjectId = project.Id,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern
        var memberAddedEvent = new MemberAddedToProjectEvent
        {
            ProjectId = request.ProjectId,
            OrganizationId = project.OrganizationId,
            UserId = invitedUser.Id,
            Role = request.Role.ToString()
        };

        var eventType = typeof(MemberAddedToProjectEvent).AssemblyQualifiedName ?? typeof(MemberAddedToProjectEvent).FullName ?? "MemberAddedToProjectEvent";
        var eventPayload = JsonSerializer.Serialize(memberAddedEvent);
        var idempotencyKey = $"member-added-{request.ProjectId}-{invitedUser.Id}-{DateTimeOffset.UtcNow.Ticks}";
        var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return projectMember.Id;
    }
}

