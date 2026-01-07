using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get AI quota usage history for admin dashboard.
/// Returns daily usage data for a date range with pagination support.
/// </summary>
public record GetAIQuotaUsageHistoryQuery : IRequest<PagedResponse<DailyUsageHistoryDto>>
{
    /// <summary>
    /// Organization ID (optional - if not provided, returns data for all organizations)
    /// </summary>
    public int? OrganizationId { get; init; }
    
    /// <summary>
    /// Start date for history (default: 30 days ago)
    /// </summary>
    public DateTimeOffset? StartDate { get; init; }
    
    /// <summary>
    /// End date for history (default: now)
    /// </summary>
    public DateTimeOffset? EndDate { get; init; }
    
    /// <summary>
    /// Page number (default: 1, minimum: 1)
    /// </summary>
    public int Page { get; init; } = 1;
    
    /// <summary>
    /// Page size (default: 20, minimum: 1, maximum: 100)
    /// </summary>
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// DTO for AI quota usage history (deprecated - use PagedResponse instead).
/// </summary>
[Obsolete("Use PagedResponse<DailyUsageHistoryDto> instead")]
public record AIQuotaUsageHistoryDto(
    List<DailyUsageHistoryDto> DailyUsage,
    int TotalDays,
    int TotalRequests,
    int TotalTokens,
    int TotalDecisions,
    decimal TotalCost
);

/// <summary>
/// DTO for daily usage history entry.
/// </summary>
public record DailyUsageHistoryDto(
    DateTime Date,
    int Requests,
    int Tokens,
    int Decisions,
    decimal Cost
);

