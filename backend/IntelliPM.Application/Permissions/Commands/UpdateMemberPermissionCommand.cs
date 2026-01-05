using MediatR;
using IntelliPM.Application.Permissions.DTOs;

namespace IntelliPM.Application.Permissions.Commands;

/// <summary>
/// Command to update a member's role and/or permissions (Admin only - own organization).
/// </summary>
public record UpdateMemberPermissionCommand : IRequest<MemberPermissionDto>
{
    public int UserId { get; init; }
    public string? GlobalRole { get; init; } // Optional: update role
    public List<int>? PermissionIds { get; init; } // Optional: explicit permission IDs (if provided, will update RolePermissions)
}

