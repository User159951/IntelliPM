namespace IntelliPM.Application.Organizations.DTOs;

/// <summary>
/// DTO for organization permission policy information.
/// </summary>
public record OrganizationPermissionPolicyDto(
    int Id,
    int OrganizationId,
    string OrganizationName,
    string OrganizationCode,
    List<string> AllowedPermissions,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

/// <summary>
/// Request DTO for updating organization permission policy.
/// </summary>
public record UpdateOrganizationPermissionPolicyRequest(
    List<string> AllowedPermissions,
    bool? IsActive
);

