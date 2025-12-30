using IntelliPM.Domain.Interfaces;
using System.Text.Json;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Tracks AI usage limits and billing per organization.
/// Enforces quota limits, tracks usage over time, and supports different tier levels.
/// </summary>
public class AIQuota : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }

    // Quota tier
    public string TierName { get; set; } = "Free"; // "Free", "Pro", "Enterprise", "Custom"
    public bool IsActive { get; set; } = true;

    // Current period
    public DateTimeOffset PeriodStartDate { get; set; }
    public DateTimeOffset PeriodEndDate { get; set; }

    // Limits (per period)
    public int MaxTokensPerPeriod { get; set; }
    public int MaxRequestsPerPeriod { get; set; }
    public int MaxDecisionsPerPeriod { get; set; }
    public decimal MaxCostPerPeriod { get; set; } // In USD

    // Current usage
    public int TokensUsed { get; set; } = 0;
    public int RequestsUsed { get; set; } = 0;
    public int DecisionsMade { get; set; } = 0;
    public decimal CostAccumulated { get; set; } = 0m;

    // Usage breakdown by agent type
    public string UsageByAgentJson { get; set; } = "{}"; // JSON: { "ProductAgent": { tokens: 100, requests: 5 }, ... }
    public string UsageByDecisionTypeJson { get; set; } = "{}"; // JSON: { "RiskDetection": { tokens: 50, decisions: 2 }, ... }

    // Quota enforcement
    public bool EnforceQuota { get; set; } = true;
    public bool IsQuotaExceeded { get; set; } = false;
    public DateTimeOffset? QuotaExceededAt { get; set; }
    public string? QuotaExceededReason { get; set; }

    // Alerts
    public decimal AlertThresholdPercentage { get; set; } = 80m; // Alert at 80%
    public bool AlertSent { get; set; } = false;
    public DateTimeOffset? AlertSentAt { get; set; }

    // Overage (if allowed)
    public bool AllowOverage { get; set; } = false;
    public decimal OverageRate { get; set; } = 0m; // Cost per token over limit
    public int OverageTokensUsed { get; set; } = 0;
    public decimal OverageCost { get; set; } = 0m;

    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastResetAt { get; set; }

    // Billing
    public string? BillingReferenceId { get; set; } // External billing system reference
    public bool IsPaid { get; set; } = true; // False if payment failed

    // Navigation properties
    public Organization Organization { get; set; } = null!;

    // Helper methods for JSON serialization
    public Dictionary<string, AgentUsage> GetUsageByAgent()
    {
        return JsonSerializer.Deserialize<Dictionary<string, AgentUsage>>(UsageByAgentJson) ?? new();
    }

    public void SetUsageByAgent(Dictionary<string, AgentUsage> usage)
    {
        UsageByAgentJson = JsonSerializer.Serialize(usage);
    }

    public Dictionary<string, DecisionTypeUsage> GetUsageByDecisionType()
    {
        return JsonSerializer.Deserialize<Dictionary<string, DecisionTypeUsage>>(UsageByDecisionTypeJson) ?? new();
    }

    public void SetUsageByDecisionType(Dictionary<string, DecisionTypeUsage> usage)
    {
        UsageByDecisionTypeJson = JsonSerializer.Serialize(usage);
    }

    public void RecordUsage(int tokens, string agentType, string decisionType, decimal cost)
    {
        TokensUsed += tokens;
        RequestsUsed += 1;
        DecisionsMade += 1;
        CostAccumulated += cost;

        // Update agent usage
        var agentUsage = GetUsageByAgent();
        if (!agentUsage.ContainsKey(agentType))
        {
            agentUsage[agentType] = new AgentUsage();
        }
        agentUsage[agentType].Tokens += tokens;
        agentUsage[agentType].Requests += 1;
        SetUsageByAgent(agentUsage);

        // Update decision type usage
        var decisionUsage = GetUsageByDecisionType();
        if (!decisionUsage.ContainsKey(decisionType))
        {
            decisionUsage[decisionType] = new DecisionTypeUsage();
        }
        decisionUsage[decisionType].Tokens += tokens;
        decisionUsage[decisionType].Decisions += 1;
        SetUsageByDecisionType(decisionUsage);

        // Check if quota exceeded
        CheckQuotaExceeded();

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void CheckQuotaExceeded()
    {
        if (!EnforceQuota) return;

        var reasons = new List<string>();

        if (TokensUsed >= MaxTokensPerPeriod)
            reasons.Add($"Token limit exceeded ({TokensUsed}/{MaxTokensPerPeriod})");

        if (RequestsUsed >= MaxRequestsPerPeriod)
            reasons.Add($"Request limit exceeded ({RequestsUsed}/{MaxRequestsPerPeriod})");

        if (DecisionsMade >= MaxDecisionsPerPeriod)
            reasons.Add($"Decision limit exceeded ({DecisionsMade}/{MaxDecisionsPerPeriod})");

        if (CostAccumulated >= MaxCostPerPeriod)
            reasons.Add($"Cost limit exceeded (${CostAccumulated}/${MaxCostPerPeriod})");

        if (reasons.Any())
        {
            IsQuotaExceeded = true;
            QuotaExceededAt = DateTimeOffset.UtcNow;
            QuotaExceededReason = string.Join("; ", reasons);
        }
    }

    public bool ShouldSendAlert()
    {
        if (AlertSent) return false;

        var tokenPercentage = MaxTokensPerPeriod > 0 ? (decimal)TokensUsed / MaxTokensPerPeriod * 100 : 0;
        var requestPercentage = MaxRequestsPerPeriod > 0 ? (decimal)RequestsUsed / MaxRequestsPerPeriod * 100 : 0;
        var costPercentage = MaxCostPerPeriod > 0 ? CostAccumulated / MaxCostPerPeriod * 100 : 0;

        return tokenPercentage >= AlertThresholdPercentage ||
               requestPercentage >= AlertThresholdPercentage ||
               costPercentage >= AlertThresholdPercentage;
    }

    public void MarkAlertSent()
    {
        AlertSent = true;
        AlertSentAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetQuota()
    {
        TokensUsed = 0;
        RequestsUsed = 0;
        DecisionsMade = 0;
        CostAccumulated = 0;
        OverageTokensUsed = 0;
        OverageCost = 0;
        IsQuotaExceeded = false;
        QuotaExceededAt = null;
        QuotaExceededReason = null;
        AlertSent = false;
        AlertSentAt = null;
        UsageByAgentJson = "{}";
        UsageByDecisionTypeJson = "{}";

        PeriodStartDate = DateTimeOffset.UtcNow;
        PeriodEndDate = DateTimeOffset.UtcNow.AddDays(Domain.Constants.AIQuotaConstants.QuotaPeriodDays);
        LastResetAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public QuotaStatus GetQuotaStatus()
    {
        return new QuotaStatus
        {
            TokensUsed = TokensUsed,
            TokensLimit = MaxTokensPerPeriod,
            TokensPercentage = MaxTokensPerPeriod > 0 ? (decimal)TokensUsed / MaxTokensPerPeriod * 100 : 0,
            RequestsUsed = RequestsUsed,
            RequestsLimit = MaxRequestsPerPeriod,
            RequestsPercentage = MaxRequestsPerPeriod > 0 ? (decimal)RequestsUsed / MaxRequestsPerPeriod * 100 : 0,
            CostAccumulated = CostAccumulated,
            CostLimit = MaxCostPerPeriod,
            CostPercentage = MaxCostPerPeriod > 0 ? CostAccumulated / MaxCostPerPeriod * 100 : 0,
            IsExceeded = IsQuotaExceeded,
            DaysRemaining = (PeriodEndDate.Date - DateTimeOffset.UtcNow.Date).Days
        };
    }
}

/// <summary>
/// Usage statistics for a specific agent type.
/// </summary>
public class AgentUsage
{
    public int Tokens { get; set; }
    public int Requests { get; set; }
}

/// <summary>
/// Usage statistics for a specific decision type.
/// </summary>
public class DecisionTypeUsage
{
    public int Tokens { get; set; }
    public int Decisions { get; set; }
}

/// <summary>
/// Current quota status with usage percentages.
/// </summary>
public class QuotaStatus
{
    public int TokensUsed { get; set; }
    public int TokensLimit { get; set; }
    public decimal TokensPercentage { get; set; }
    public int RequestsUsed { get; set; }
    public int RequestsLimit { get; set; }
    public decimal RequestsPercentage { get; set; }
    public decimal CostAccumulated { get; set; }
    public decimal CostLimit { get; set; }
    public decimal CostPercentage { get; set; }
    public bool IsExceeded { get; set; }
    public int DaysRemaining { get; set; }
}

