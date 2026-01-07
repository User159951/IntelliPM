using MediatR;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get AI quota breakdown by agent type and decision type.
/// Provides detailed breakdown for admin dashboard.
/// </summary>
public record GetAIQuotaBreakdownQuery : IRequest<AIQuotaBreakdownDto>
{
    /// <summary>
    /// Organization ID (optional - if not provided, returns data for all organizations)
    /// </summary>
    public int? OrganizationId { get; init; }
    
    /// <summary>
    /// Period for breakdown: "day", "week", "month" (default: "month")
    /// </summary>
    public string Period { get; init; } = "month";
    
    /// <summary>
    /// Start date for breakdown (optional - defaults based on period)
    /// </summary>
    public DateTimeOffset? StartDate { get; init; }
    
    /// <summary>
    /// End date for breakdown (optional - defaults to now)
    /// </summary>
    public DateTimeOffset? EndDate { get; init; }
}

/// <summary>
/// DTO for AI quota breakdown.
/// </summary>
public record AIQuotaBreakdownDto(
    Dictionary<string, AgentBreakdownDto> ByAgent,
    Dictionary<string, DecisionTypeBreakdownDto> ByDecisionType,
    PeriodSummaryDto Summary
);

/// <summary>
/// DTO for agent breakdown.
/// </summary>
public record AgentBreakdownDto(
    string AgentType,
    int Requests,
    int Tokens,
    int Decisions,
    decimal Cost,
    decimal PercentageOfTotalTokens
);

/// <summary>
/// DTO for decision type breakdown.
/// </summary>
public record DecisionTypeBreakdownDto(
    string DecisionType,
    int Decisions,
    int Tokens,
    decimal Cost,
    decimal PercentageOfTotalDecisions
);

/// <summary>
/// DTO for period summary.
/// </summary>
public record PeriodSummaryDto(
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int TotalRequests,
    int TotalTokens,
    int TotalDecisions,
    decimal TotalCost
);

