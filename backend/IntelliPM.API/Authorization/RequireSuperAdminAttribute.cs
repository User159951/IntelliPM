using Microsoft.AspNetCore.Authorization;

namespace IntelliPM.API.Authorization;

/// <summary>
/// Authorization attribute that requires the user to have SuperAdmin role.
/// Only SuperAdmin users can access endpoints marked with this attribute.
/// </summary>
public class RequireSuperAdminAttribute : AuthorizeAttribute
{
    public RequireSuperAdminAttribute()
    {
        Roles = "SuperAdmin";
    }
}

