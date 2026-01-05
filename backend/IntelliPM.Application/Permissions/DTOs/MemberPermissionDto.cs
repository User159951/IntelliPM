namespace IntelliPM.Application.Permissions.DTOs;

/// <summary>
/// DTO for member permission information.
/// </summary>
public record MemberPermissionDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string GlobalRole,
    int OrganizationId,
    string OrganizationName,
    List<string> Permissions, // Permissions derived from role
    List<int> PermissionIds // Permission IDs for role-based assignment
);

/// <summary>
/// Request DTO for updating member permissions.
/// </summary>
public record UpdateMemberPermissionRequest(
    string? GlobalRole, // Optional: update role
    List<int>? PermissionIds // Optional: explicit permission IDs (if role-based, derive from role)
);

