using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Command to update a user's global role within the organization (Admin only - own organization).
/// Admin can only set Admin or User roles, not SuperAdmin.
/// </summary>
public record UpdateUserGlobalRoleCommand : IRequest<UpdateUserGlobalRoleResponse>
{
    public int UserId { get; init; }
    public GlobalRole GlobalRole { get; init; }
}

/// <summary>
/// Response for updating a user's global role.
/// </summary>
public record UpdateUserGlobalRoleResponse(
    int UserId,
    GlobalRole GlobalRole,
    string Message
);

