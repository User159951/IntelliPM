using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IntelliPM.API.Authorization;

/// <summary>
/// Custom authorization policy provider that creates policies dynamically for permissions.
/// When RequirePermissionAttribute is used with Policy = "Permission:xxx", this provider
/// creates a policy with the PermissionRequirement.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private const string PermissionPolicyPrefix = "Permission:";

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if this is a permission-based policy
        if (policyName.StartsWith(PermissionPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring(PermissionPolicyPrefix.Length);
            
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default policy provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}

