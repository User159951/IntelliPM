using IntelliPM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace IntelliPM.API.Authorization;

/// <summary>
/// Authorization handler for RequirePermissionAttribute.
/// Checks if the current user has the required permission via IPermissionService.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IPermissionService permissionService,
        ICurrentUserService currentUserService,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = _currentUserService.GetUserId();
        
        if (userId == 0)
        {
            _logger.LogWarning("Permission check failed: User ID not found");
            return;
        }

        try
        {
            var hasPermission = await _permissionService.HasPermissionAsync(userId, requirement.Permission);
            
            if (hasPermission)
            {
                _logger.LogDebug("User {UserId} has permission {Permission}", userId, requirement.Permission);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} does not have permission {Permission}", userId, requirement.Permission);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", requirement.Permission, userId);
            // Fail closed - if we can't check the permission, deny access
        }
    }
}

/// <summary>
/// Requirement for permission-based authorization.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}

