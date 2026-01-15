using Microsoft.AspNetCore.Authorization;

namespace IntelliPM.API.Authorization;

/// <summary>
/// Authorization attribute that requires the user to have Admin or SuperAdmin role.
/// Admin users can only access resources from their own organization.
/// SuperAdmin users can access resources from all organizations.
/// </summary>
public class RequireAdminAttribute : AuthorizeAttribute
{
    public RequireAdminAttribute()
    {
        Roles = "Admin,SuperAdmin";
    }
}
