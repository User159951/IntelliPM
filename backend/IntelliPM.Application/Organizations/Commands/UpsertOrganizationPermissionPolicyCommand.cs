using MediatR;
using IntelliPM.Application.Organizations.DTOs;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Command to upsert (create or update) organization permission policy (SuperAdmin only).
/// </summary>
public record UpsertOrganizationPermissionPolicyCommand : IRequest<OrganizationPermissionPolicyDto>
{
    public int OrganizationId { get; init; }
    public List<string> AllowedPermissions { get; init; } = new();
    public bool? IsActive { get; init; }
}

