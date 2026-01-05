using MediatR;
using IntelliPM.Application.Organizations.DTOs;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Query to get organization permission policy by organization ID (SuperAdmin only).
/// </summary>
public record GetOrganizationPermissionPolicyQuery : IRequest<OrganizationPermissionPolicyDto>
{
    public int OrganizationId { get; init; }
}

