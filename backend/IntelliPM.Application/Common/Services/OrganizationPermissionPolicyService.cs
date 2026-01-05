using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IntelliPM.Application.Common.Services;

/// <summary>
/// Service for checking and enforcing organization permission policies.
/// </summary>
public class OrganizationPermissionPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrganizationPermissionPolicyService> _logger;

    public OrganizationPermissionPolicyService(
        IUnitOfWork unitOfWork,
        ILogger<OrganizationPermissionPolicyService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Gets the organization permission policy for the given organization.
    /// Returns null if no policy exists (default = allow all).
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
    /// Returns true if:
    /// - No policy exists (default = allow all)
    /// - Policy is inactive (allow all)
    /// - Policy is active and contains the permission
    /// </summary>
    public async System.Threading.Tasks.Task<bool> IsPermissionAllowedAsync(int organizationId, string permission, CancellationToken ct = default)
    {
        var policy = await GetPolicyAsync(organizationId, ct);
        
        if (policy == null)
        {
            return true; // No policy = allow all (default behavior)
        }

        return policy.IsPermissionAllowed(permission);
    }

    /// <summary>
    /// Validates that all given permissions are allowed for the organization.
    /// Throws ApplicationException if any permission is not allowed.
    /// </summary>
    public async System.Threading.Tasks.Task ValidatePermissionsAsync(int organizationId, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        var policy = await GetPolicyAsync(organizationId, ct);
        
        if (policy == null || !policy.IsActive)
        {
            return; // No policy or inactive = allow all
        }

        var allowedPermissions = policy.GetAllowedPermissions();
        if (allowedPermissions.Count == 0)
        {
            return; // Empty policy = allow all
        }

        var permissionList = permissions.ToList();
        var disallowedPermissions = permissionList
            .Where(p => !policy.IsPermissionAllowed(p))
            .ToList();

        if (disallowedPermissions.Any())
        {
            throw new Application.Common.Exceptions.ApplicationException(
                $"The following permissions are not allowed for this organization: {string.Join(", ", disallowedPermissions)}");
        }
    }

    /// <summary>
    /// Gets all allowed permissions for an organization.
    /// Returns empty list if all permissions are allowed (no policy, inactive policy, or empty policy).
    /// </summary>
    public async System.Threading.Tasks.Task<List<string>> GetAllowedPermissionsAsync(int organizationId, CancellationToken ct = default)
    {
        var policy = await GetPolicyAsync(organizationId, ct);
        
        if (policy == null || !policy.IsActive)
        {
            return new List<string>(); // Empty list = all permissions allowed
        }

        var allowed = policy.GetAllowedPermissions();
        if (allowed.Count == 0)
        {
            return new List<string>(); // Empty list = all permissions allowed
        }

        return allowed;
    }
}

