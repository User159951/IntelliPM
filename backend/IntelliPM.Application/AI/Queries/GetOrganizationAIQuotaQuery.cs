using MediatR;
using IntelliPM.Application.AI.DTOs;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get organization AI quota by organization ID (SuperAdmin only).
/// </summary>
public record GetOrganizationAIQuotaQuery : IRequest<OrganizationAIQuotaDto>
{
    public int OrganizationId { get; init; }
}

