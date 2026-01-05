using MediatR;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.AI.DTOs;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get a paginated list of organization AI quotas (SuperAdmin only).
/// </summary>
public record GetOrganizationAIQuotasQuery : IRequest<PagedResponse<OrganizationAIQuotaDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public bool? IsAIEnabled { get; init; }
}

