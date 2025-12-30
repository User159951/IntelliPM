using Microsoft.AspNetCore.Authorization;

namespace IntelliPM.API.Authorization;

/// <summary>
/// Authorization attribute that requires a specific permission
/// Usage: [RequirePermission("projects.create")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        Policy = $"Permission:{permission}";
    }
}


