using MediatR;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.AI.DTOs;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get a paginated list of organization members with their effective AI quotas (Admin only - own organization).
/// </summary>
public record GetMemberAIQuotasQuery : IRequest<PagedResponse<MemberAIQuotaDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
}

