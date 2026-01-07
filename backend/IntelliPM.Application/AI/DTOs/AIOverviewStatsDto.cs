namespace IntelliPM.Application.AI.DTOs;

/// <summary>
/// DTO for AI overview statistics aggregated across all organizations.
/// </summary>
public record AIOverviewStatsDto
{
    public int TotalOrganizations { get; init; }
    public int OrganizationsWithAIEnabled { get; init; }
    public int OrganizationsWithAIDisabled { get; init; }
    public int TotalDecisionsLast30Days { get; init; }
    public int PendingApprovals { get; init; }
    public int ApprovedDecisions { get; init; }
    public int RejectedDecisions { get; init; }
    public double AverageConfidenceScore { get; init; }
    public decimal TotalCostLast30Days { get; init; }
    public List<TopAgentUsageDto> TopAgents { get; init; } = new();
    public List<QuotaUsageByTierDto> QuotaByTier { get; init; } = new();
}

/// <summary>
/// DTO for top agent usage statistics.
/// </summary>
public record TopAgentUsageDto
{
    public string AgentType { get; init; } = string.Empty;
    public int DecisionCount { get; init; }
    public long TotalTokensUsed { get; init; }
}

/// <summary>
/// DTO for quota usage breakdown by tier.
/// </summary>
public record QuotaUsageByTierDto
{
    public string TierName { get; init; } = string.Empty;
    public int OrganizationCount { get; init; }
    public double AverageUsagePercentage { get; init; }
    public int ExceededCount { get; init; }
}

