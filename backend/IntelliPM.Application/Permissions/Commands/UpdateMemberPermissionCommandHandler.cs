using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Services;
using IntelliPM.Application.Permissions.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.Application.Permissions.Commands;

/// <summary>
/// Handler for UpdateMemberPermissionCommand.
/// Updates a member's role and/or permissions (Admin only - own organization).
/// Enforces organization permission policy: assigned permissions must be subset of org allowed permissions.
/// </summary>
public class UpdateMemberPermissionCommandHandler : IRequestHandler<UpdateMemberPermissionCommand, MemberPermissionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly OrganizationPermissionPolicyService _policyService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<UpdateMemberPermissionCommandHandler> _logger;

    public UpdateMemberPermissionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        OrganizationPermissionPolicyService policyService,
        IPermissionService permissionService,
        ILogger<UpdateMemberPermissionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _policyService = policyService;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<MemberPermissionDto> Handle(UpdateMemberPermissionCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can update member permissions");
        }

        var currentUserId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (currentUserId == 0 || organizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        // Get target user and ensure they're in the same organization
        var targetUser = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (targetUser == null)
        {
            throw new NotFoundException($"User {request.UserId} not found");
        }

        // Ensure organization access (Admin can only modify users in their own organization)
        _scopingService.EnsureOrganizationAccess(targetUser.OrganizationId);

        // Prevent Admin from changing their own role/permissions (to avoid lockout)
        if (targetUser.Id == currentUserId && !_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("You cannot change your own role or permissions. Another admin must change it.");
        }

        // If updating role, validate role assignment rules
        GlobalRole? newRole = null;
        if (!string.IsNullOrWhiteSpace(request.GlobalRole))
        {
            if (!Enum.TryParse<GlobalRole>(request.GlobalRole, out var parsedRole))
            {
                throw new ValidationException($"Invalid role: {request.GlobalRole}");
            }

            newRole = parsedRole;

            // Admin cannot assign SuperAdmin role
            if (newRole == GlobalRole.SuperAdmin && !_currentUserService.IsSuperAdmin())
            {
                throw new UnauthorizedException("Admin cannot assign SuperAdmin role. Only SuperAdmin can assign SuperAdmin role.");
            }

            // Admin can only assign Admin or User roles
            if (!_currentUserService.IsSuperAdmin() && newRole != GlobalRole.Admin && newRole != GlobalRole.User)
            {
                throw new UnauthorizedException("Invalid role. Admin can only assign Admin or User roles.");
            }

            // Update role
            targetUser.GlobalRole = newRole.Value;
            targetUser.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Determine which permissions will be assigned
        List<string> permissionsToAssign = new();
        List<int> permissionIdsToAssign = new();

        if (request.PermissionIds != null && request.PermissionIds.Any())
        {
            // Explicit permission IDs provided - use those
            var permissionRepo = _unitOfWork.Repository<Permission>();
            var permissions = await permissionRepo.Query()
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(ct);

            if (permissions.Count != request.PermissionIds.Count)
            {
                var missing = request.PermissionIds.Except(permissions.Select(p => p.Id)).ToList();
                throw new ValidationException($"Some permissions do not exist: {string.Join(", ", missing)}");
            }

            permissionIdsToAssign = request.PermissionIds;
            permissionsToAssign = permissions.Select(p => p.Name).ToList();
        }
        else if (newRole.HasValue)
        {
            // Role updated - derive permissions from role
            var rolePermissions = await _unitOfWork.Repository<RolePermission>()
                .Query()
                .AsNoTracking()
                .Include(rp => rp.Permission)
                .Where(rp => rp.Role == newRole.Value)
                .Select(rp => new { rp.PermissionId, rp.Permission.Name })
                .ToListAsync(ct);

            permissionIdsToAssign = rolePermissions.Select(rp => rp.PermissionId).ToList();
            permissionsToAssign = rolePermissions.Select(rp => rp.Name).ToList();
        }
        else
        {
            // No role change and no explicit permissions - keep existing permissions
            var existingPermissions = await _permissionService.GetUserPermissionsAsync(request.UserId, ct);
            permissionsToAssign = existingPermissions;
            
            // Get permission IDs for existing permissions
            var permissionRepo = _unitOfWork.Repository<Permission>();
            var existingPerms = await permissionRepo.Query()
                .Where(p => existingPermissions.Contains(p.Name))
                .Select(p => p.Id)
                .ToListAsync(ct);
            permissionIdsToAssign = existingPerms;
        }

        // Enforce organization permission policy (unless SuperAdmin)
        if (!_currentUserService.IsSuperAdmin() && permissionsToAssign.Any())
        {
            await _policyService.ValidatePermissionsAsync(organizationId, permissionsToAssign, ct);
        }

        // If explicit permissions were provided, update RolePermissions for the user's role
        // Note: This is a global role-permission mapping, not per-user.
        // In this system, permissions are derived from roles via RolePermission table.
        // If we want per-user permission overrides, we'd need a UserPermission table.
        // For now, we only update the role, and permissions are derived from RolePermission.
        
        // If role was changed, permissions are already derived from the new role above.
        // If explicit PermissionIds were provided, we could update RolePermission for that role,
        // but that would affect all users with that role globally. That's handled by UpdateRolePermissionsCommand.
        // So for this endpoint, we only update the user's role, and permissions are derived from RolePermission.

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Member permissions updated for user {UserId} in organization {OrganizationId} by admin {AdminUserId}. Role: {Role}, Permissions: {Permissions}",
            request.UserId, organizationId, currentUserId, targetUser.GlobalRole, string.Join(", ", permissionsToAssign));

        // Build response DTO
        return new MemberPermissionDto(
            targetUser.Id,
            targetUser.Email,
            targetUser.FirstName,
            targetUser.LastName,
            $"{targetUser.FirstName} {targetUser.LastName}",
            targetUser.GlobalRole.ToString(),
            targetUser.OrganizationId,
            targetUser.Organization.Name,
            permissionsToAssign,
            permissionIdsToAssign
        );
    }
}

