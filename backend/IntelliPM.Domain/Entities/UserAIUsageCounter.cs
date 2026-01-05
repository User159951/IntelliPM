using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Tracks per-user AI usage within a period.
/// Aggregated from AIDecisionLog entries for efficient quota checking.
/// </summary>
public class UserAIUsageCounter : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int UserId { get; set; }

    // Period
    public DateTimeOffset PeriodStartDate { get; set; }
    public DateTimeOffset PeriodEndDate { get; set; }

    // Usage tracking
    public int TokensUsed { get; set; } = 0;
    public int RequestsUsed { get; set; } = 0;
    public int DecisionsMade { get; set; } = 0;
    public decimal CostAccumulated { get; set; } = 0m;

    // Metadata
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastAggregatedAt { get; set; } // When was this last updated from AIDecisionLog?

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public User User { get; set; } = null!;

    /// <summary>
    /// Records usage from a decision log entry.
    /// </summary>
    public void RecordUsage(int tokens, decimal cost)
    {
        TokensUsed += tokens;
        RequestsUsed += 1;
        DecisionsMade += 1;
        CostAccumulated += cost;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Resets usage for a new period.
    /// </summary>
    public void ResetForNewPeriod(DateTimeOffset periodStart, DateTimeOffset periodEnd)
    {
        TokensUsed = 0;
        RequestsUsed = 0;
        DecisionsMade = 0;
        CostAccumulated = 0m;
        PeriodStartDate = periodStart;
        PeriodEndDate = periodEnd;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

