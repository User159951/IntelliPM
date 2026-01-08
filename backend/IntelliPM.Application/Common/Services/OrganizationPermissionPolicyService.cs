using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntelliPM.Application.Common.Services;

/// <summary>
/// Service for checking and enforcing organization permission policies.
/// SECURITY: Implements deny-by-default security model - all permissions must be explicitly allowed.
/// </summary>
public class OrganizationPermissionPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrganizationPermissionPolicyService> _logger;
    private readonly ICurrentUserService? _currentUserService;

    public OrganizationPermissionPolicyService(
        IUnitOfWork unitOfWork,
        ILogger<OrganizationPermissionPolicyService> logger,
        ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the organization permission policy for the given organization.
    /// Returns null if no policy exists (deny by default).
    /// </summary>
    public async System.Threading.Tasks.Task<OrganizationPermissionPolicy?> GetPolicyAsync(int organizationId, CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<OrganizationPermissionPolicy>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId, ct);
    }

    /// <summary>
    /// Checks if a permission is allowed for the given organization.
    /// SECURITY: Deny by default - returns false if:
    /// - No policy exists
    /// - Policy is inactive
    /// - Policy is empty
    /// - Permission is not explicitly in the allowed list
    /// Only returns true if policy is active AND explicitly contains the permission.
    /// </summary>
    public async System.Threading.Tasks.Task<bool> IsPermissionAllowedAsync(int organizationId, string permission, CancellationToken ct = default)
    {
        var policy = await GetPolicyAsync(organizationId, ct);
        
        // SECURITY: Deny by default - no policy means no permissions allowed
        if (policy == null)
        {
            await LogPermissionCheckAsync(organizationId, permission, false, "No policy exists", ct);
            return false;
        }

        var isAllowed = policy.IsPermissionAllowed(permission);
        var reason = isAllowed 
            ? "Permission explicitly allowed in policy" 
            : policy.IsActive 
                ? "Permission not in allowed list" 
                : "Policy is inactive";
        
        await LogPermissionCheckAsync(organizationId, permission, isAllowed, reason, ct);
        return isAllowed;
    }

    /// <summary>
    /// Validates that all given permissions are allowed for the organization.
    /// SECURITY: Throws ForbiddenException (403) if policy is missing or any permission is not allowed.
    /// </summary>
    public async System.Threading.Tasks.Task ValidatePermissionsAsync(int organizationId, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        var policy = await GetPolicyAsync(organizationId, ct);
        var permissionList = permissions.ToList();
        
        // SECURITY: Deny by default - missing policy means no permissions allowed
        if (policy == null)
        {
            var message = $"Permission policy not found for organization {organizationId}. Access denied (deny by default).";
            _logger.LogWarning(
                "Permission validation failed: Organization {OrganizationId} has no policy. Requested permissions: {Permissions}",
                organizationId, string.Join(", ", permissionList));
            
            await LogPermissionValidationAsync(organizationId, permissionList, false, "No policy exists", ct);
            
            throw new Application.Common.Exceptions.ForbiddenException(
                message,
                permissionList.FirstOrDefault(),
                organizationId);
        }

        // SECURITY: Inactive policy means no permissions allowed
        if (!policy.IsActive)
        {
            var message = $"Permission policy is inactive for organization {organizationId}. Access denied (deny by default).";
            _logger.LogWarning(
                "Permission validation failed: Organization {OrganizationId} has inactive policy. Requested permissions: {Permissions}",
                organizationId, string.Join(", ", permissionList));
            
            await LogPermissionValidationAsync(organizationId, permissionList, false, "Policy is inactive", ct);
            
            throw new Application.Common.Exceptions.ForbiddenException(
                message,
                permissionList.FirstOrDefault(),
                organizationId);
        }

        var allowedPermissions = policy.GetAllowedPermissions();
        
        // SECURITY: Empty policy means no permissions allowed
        if (allowedPermissions.Count == 0)
        {
            var message = $"Permission policy is empty for organization {organizationId}. Access denied (deny by default).";
            _logger.LogWarning(
                "Permission validation failed: Organization {OrganizationId} has empty policy. Requested permissions: {Permissions}",
                organizationId, string.Join(", ", permissionList));
            
            await LogPermissionValidationAsync(organizationId, permissionList, false, "Policy is empty", ct);
            
            throw new Application.Common.Exceptions.ForbiddenException(
                message,
                permissionList.FirstOrDefault(),
                organizationId);
        }

        // Check each permission
        var disallowedPermissions = permissionList
            .Where(p => !policy.IsPermissionAllowed(p))
            .ToList();

        if (disallowedPermissions.Any())
        {
            var message = $"The following permissions are not allowed for this organization: {string.Join(", ", disallowedPermissions)}";
            _logger.LogWarning(
                "Permission validation failed: Organization {OrganizationId} denied permissions {Permissions}",
                organizationId, string.Join(", ", disallowedPermissions));
            
            await LogPermissionValidationAsync(organizationId, permissionList, false, $"Denied: {string.Join(", ", disallowedPermissions)}", ct);
            
            throw new Application.Common.Exceptions.ForbiddenException(
                message,
                disallowedPermissions.First(),
                organizationId);
        }

        // All permissions allowed - log success
        await LogPermissionValidationAsync(organizationId, permissionList, true, "All permissions allowed", ct);
    }

    /// <summary>
    /// Gets all allowed permissions for an organization.
    /// SECURITY: Returns empty list if no policy exists, policy is inactive, or policy is empty (deny by default).
    /// </summary>
    public async System.Threading.Tasks.Task<List<string>> GetAllowedPermissionsAsync(int organizationId, CancellationToken ct = default)
    {
        var policy = await GetPolicyAsync(organizationId, ct);
        
        // SECURITY: Deny by default - no policy means no permissions
        if (policy == null)
        {
            return new List<string>();
        }

        // SECURITY: Inactive policy means no permissions
        if (!policy.IsActive)
        {
            return new List<string>();
        }

        var allowed = policy.GetAllowedPermissions();
        
        // Return the explicitly allowed permissions (empty list means deny all)
        return allowed;
    }

    /// <summary>
    /// Logs a permission check to audit log for security tracking.
    /// </summary>
    private async System.Threading.Tasks.Task LogPermissionCheckAsync(
        int organizationId, 
        string permission, 
        bool allowed, 
        string reason, 
        CancellationToken ct)
    {
        try
        {
            var userId = _currentUserService?.GetUserId() ?? 0;
            
            var auditLog = new AuditLog
            {
                UserId = userId > 0 ? userId : null,
                Action = allowed ? "PermissionCheck.Allowed" : "PermissionCheck.Denied",
                EntityType = "OrganizationPermissionPolicy",
                EntityId = organizationId,
                EntityName = $"Organization {organizationId}",
                Changes = JsonSerializer.Serialize(new
                {
                    Permission = permission,
                    OrganizationId = organizationId,
                    Allowed = allowed,
                    Reason = reason,
                    Timestamp = DateTimeOffset.UtcNow
                }),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var auditLogRepo = _unitOfWork.Repository<AuditLog>();
            await auditLogRepo.AddAsync(auditLog, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - audit logging should not break permission checks
            _logger.LogError(ex, 
                "Failed to log permission check audit: OrganizationId={OrganizationId}, Permission={Permission}, Allowed={Allowed}",
                organizationId, permission, allowed);
        }
    }

    /// <summary>
    /// Logs a permission validation (multiple permissions) to audit log for security tracking.
    /// </summary>
    private async System.Threading.Tasks.Task LogPermissionValidationAsync(
        int organizationId, 
        List<string> permissions, 
        bool allowed, 
        string reason, 
        CancellationToken ct)
    {
        try
        {
            var userId = _currentUserService?.GetUserId() ?? 0;
            
            var auditLog = new AuditLog
            {
                UserId = userId > 0 ? userId : null,
                Action = allowed ? "PermissionValidation.Allowed" : "PermissionValidation.Denied",
                EntityType = "OrganizationPermissionPolicy",
                EntityId = organizationId,
                EntityName = $"Organization {organizationId}",
                Changes = JsonSerializer.Serialize(new
                {
                    Permissions = permissions,
                    OrganizationId = organizationId,
                    Allowed = allowed,
                    Reason = reason,
                    Timestamp = DateTimeOffset.UtcNow
                }),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var auditLogRepo = _unitOfWork.Repository<AuditLog>();
            await auditLogRepo.AddAsync(auditLog, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - audit logging should not break permission checks
            _logger.LogError(ex, 
                "Failed to log permission validation audit: OrganizationId={OrganizationId}, Permissions={Permissions}, Allowed={Allowed}",
                organizationId, string.Join(", ", permissions), allowed);
        }
    }
}

