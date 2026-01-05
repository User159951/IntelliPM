using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Per-user AI quota override. Allows admins to set custom quota limits for individual users.
/// Nullable fields mean "use organization default" (from AIQuota).
/// </summary>
public class UserAIQuotaOverride : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int UserId { get; set; }

    // Period (matches organization quota period)
    public DateTimeOffset PeriodStartDate { get; set; }
    public DateTimeOffset PeriodEndDate { get; set; }

    // Override limits (nullable = use org default)
    public int? MaxTokensPerPeriod { get; set; }
    public int? MaxRequestsPerPeriod { get; set; }
    public int? MaxDecisionsPerPeriod { get; set; }
    public decimal? MaxCostPerPeriod { get; set; }

    // Metadata
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int? CreatedByUserId { get; set; } // Admin who created/updated this override
    public string? Reason { get; set; } // Audit trail: why was this override set?

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Computes effective quota limits by combining override with organization defaults.
    /// </summary>
    public EffectiveQuotaLimits GetEffectiveLimits(AIQuota orgQuota)
    {
        return new EffectiveQuotaLimits
        {
            MaxTokensPerPeriod = MaxTokensPerPeriod ?? orgQuota.MaxTokensPerPeriod,
            MaxRequestsPerPeriod = MaxRequestsPerPeriod ?? orgQuota.MaxRequestsPerPeriod,
            MaxDecisionsPerPeriod = MaxDecisionsPerPeriod ?? orgQuota.MaxDecisionsPerPeriod,
            MaxCostPerPeriod = MaxCostPerPeriod ?? orgQuota.MaxCostPerPeriod,
            HasOverride = MaxTokensPerPeriod.HasValue || MaxRequestsPerPeriod.HasValue ||
                         MaxDecisionsPerPeriod.HasValue || MaxCostPerPeriod.HasValue
        };
    }
}

/// <summary>
/// Effective quota limits for a user (override + org default).
/// </summary>
public class EffectiveQuotaLimits
{
    public int MaxTokensPerPeriod { get; set; }
    public int MaxRequestsPerPeriod { get; set; }
    public int MaxDecisionsPerPeriod { get; set; }
    public decimal MaxCostPerPeriod { get; set; }
    public bool HasOverride { get; set; }
}

