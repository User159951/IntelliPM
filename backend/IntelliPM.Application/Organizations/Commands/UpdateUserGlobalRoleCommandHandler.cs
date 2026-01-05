using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Services;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Handler for UpdateUserGlobalRoleCommand.
/// Updates a user's global role within the organization (Admin only - own organization).
/// Admin can only set Admin or User roles, not SuperAdmin.
/// </summary>
public class UpdateUserGlobalRoleCommandHandler : IRequestHandler<UpdateUserGlobalRoleCommand, UpdateUserGlobalRoleResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly ILogger<UpdateUserGlobalRoleCommandHandler> _logger;

    public UpdateUserGlobalRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        ILogger<UpdateUserGlobalRoleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _logger = logger;
    }

    public async Task<UpdateUserGlobalRoleResponse> Handle(UpdateUserGlobalRoleCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can update user roles");
        }

        // Admin cannot assign SuperAdmin role
        if (request.GlobalRole == GlobalRole.SuperAdmin)
        {
            throw new UnauthorizedException("Admin cannot assign SuperAdmin role. Only SuperAdmin can assign SuperAdmin role.");
        }

        // Admin can only assign Admin or User roles
        if (request.GlobalRole != GlobalRole.Admin && request.GlobalRole != GlobalRole.User)
        {
            throw new UnauthorizedException("Invalid role. Admin can only assign Admin or User roles.");
        }

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user == null)
        {
            throw new NotFoundException($"User {request.UserId} not found");
        }

        // Ensure organization access (Admin can only modify users in their own organization)
        _scopingService.EnsureOrganizationAccess(user.OrganizationId);

        // Prevent Admin from changing their own role (to avoid lockout)
        var currentUserId = _currentUserService.GetUserId();
        if (user.Id == currentUserId && !_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("You cannot change your own role. Another admin must change it.");
        }

        var oldRole = user.GlobalRole;
        user.GlobalRole = request.GlobalRole;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} role updated from {OldRole} to {NewRole} by admin {AdminUserId}",
            user.Id, oldRole, request.GlobalRole, currentUserId);

        return new UpdateUserGlobalRoleResponse(
            user.Id,
            user.GlobalRole,
            $"User role updated successfully from {oldRole} to {request.GlobalRole}"
        );
    }
}

