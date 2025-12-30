using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Projects.Commands;

public class ChangeMemberRoleCommandHandler : IRequestHandler<ChangeMemberRoleCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public ChangeMemberRoleCommandHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Unit> Handle(ChangeMemberRoleCommand request, CancellationToken cancellationToken)
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

        // Check if current user has permission to change roles
        if (!ProjectPermissions.CanChangeRoles(currentUserMember.Role))
            throw new UnauthorizedAccessException($"User {request.CurrentUserId} does not have permission to change member roles in this project");

        // Find the member whose role is being changed
        var memberToUpdate = project.Members.FirstOrDefault(m => m.UserId == request.UserId);
        if (memberToUpdate == null)
            throw new NotFoundException($"User {request.UserId} is not a member of project {request.ProjectId}");

        // Prevent changing the ProductOwner's role
        if (memberToUpdate.Role == ProjectRole.ProductOwner)
            throw new InvalidOperationException("Cannot change ProductOwner's role. Transfer ownership first.");

        // Prevent changing role to ProductOwner (should use transfer ownership instead)
        if (request.NewRole == ProjectRole.ProductOwner)
            throw new InvalidOperationException("Cannot assign ProductOwner role directly. Use transfer ownership instead.");

        // Store old role for activity log
        var oldRole = memberToUpdate.Role;

        // Update the role
        memberToUpdate.Role = request.NewRole;
        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        memberRepo.Update(memberToUpdate);

        // Get user info for activity log and notification
        var userRepo = _unitOfWork.Repository<User>();
        var updatedUser = await userRepo.GetByIdAsync(request.UserId, cancellationToken);
        var currentUser = await userRepo.GetByIdAsync(request.CurrentUserId, cancellationToken);

        // Create activity log
        var activityRepo = _unitOfWork.Repository<Domain.Entities.Activity>();
        await activityRepo.AddAsync(new Domain.Entities.Activity
        {
            UserId = request.CurrentUserId,
            ActivityType = "member_role_changed",
            EntityType = "project_member",
            EntityId = memberToUpdate.Id,
            EntityName = updatedUser != null ? $"{updatedUser.FirstName} {updatedUser.LastName}" : $"User {request.UserId}",
            ProjectId = request.ProjectId,
            ProjectName = project.Name,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { OldRole = oldRole.ToString(), NewRole = request.NewRole.ToString() }),
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Create notification for the user whose role changed
        var notificationRepo = _unitOfWork.Repository<Notification>();
        await notificationRepo.AddAsync(new Notification
        {
            UserId = request.UserId,
            Type = "role_changed",
            Message = $"{currentUser?.FirstName} {currentUser?.LastName} changed your role in project '{project.Name}' from {oldRole} to {request.NewRole}",
            EntityType = "project",
            EntityId = project.Id,
            ProjectId = project.Id,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

