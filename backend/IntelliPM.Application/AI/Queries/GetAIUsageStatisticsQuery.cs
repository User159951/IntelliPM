using MediatR;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get AI usage statistics for an organization.
/// </summary>
public record GetAIUsageStatisticsQuery : IRequest<AIUsageStatisticsDto>
{
    public int OrganizationId { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
}

/// <summary>
/// DTO for AI usage statistics.
/// </summary>
public record AIUsageStatisticsDto(
    int TotalTokensUsed,
    int TotalRequests,
    int TotalDecisions,
    decimal TotalCost,
    Dictionary<string, AgentUsageStatsDto> UsageByAgent,
    Dictionary<string, DecisionTypeStatsDto> UsageByDecisionType,
    List<DailyUsageDto> DailyUsage
);

/// <summary>
/// DTO for agent usage statistics.
/// </summary>
public record AgentUsageStatsDto(
    int TokensUsed,
    int RequestsCount
);

/// <summary>
/// DTO for decision type statistics.
/// </summary>
public record DecisionTypeStatsDto(
    int TokensUsed,
    int DecisionsCount
);

/// <summary>
/// DTO for daily usage.
/// </summary>
public record DailyUsageDto(
    DateTime Date,
    int TokensUsed,
    int RequestsCount,
    int DecisionsCount
);

