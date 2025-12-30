using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get all AI quotas with filtering and pagination (Admin only).
/// </summary>
public record GetAllAIQuotasQuery : IRequest<PagedResponse<AIQuotaDto>>
{
    public string? TierName { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsExceeded { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// DTO for AI quota summary.
/// </summary>
public record AIQuotaDto(
    int Id,
    int OrganizationId,
    string OrganizationName,
    string TierName,
    bool IsActive,
    QuotaUsageDto Usage,
    DateTimeOffset PeriodEndDate,
    bool IsExceeded,
    bool AlertSent
);

