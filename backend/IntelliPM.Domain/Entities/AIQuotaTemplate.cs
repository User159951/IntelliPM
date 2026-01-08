using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Template for AI quota tiers that defines default limits and settings.
/// Templates can be managed through the Admin API and used to initialize quotas for organizations.
/// </summary>
public class AIQuotaTemplate : IAggregateRoot
{
    public int Id { get; set; }

    /// <summary>
    /// Unique name of the tier (e.g., "Free", "Pro", "Enterprise").
    /// </summary>
    public string TierName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the tier and its intended use.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this template is active and can be used for new quotas.
    /// Inactive templates are kept for historical reference but cannot be assigned.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a system template that cannot be deleted.
    /// System templates (Free, Pro, Enterprise) are seeded by migrations.
    /// </summary>
    public bool IsSystemTemplate { get; set; } = false;

    // Quota limits (per period)
    public int MaxTokensPerPeriod { get; set; }
    public int MaxRequestsPerPeriod { get; set; }
    public int MaxDecisionsPerPeriod { get; set; }
    public decimal MaxCostPerPeriod { get; set; } // In USD

    // Overage settings
    public bool AllowOverage { get; set; } = false;
    public decimal OverageRate { get; set; } = 0m; // Cost per token over limit

    /// <summary>
    /// Default alert threshold percentage (e.g., 80 = alert at 80% usage).
    /// </summary>
    public decimal DefaultAlertThresholdPercentage { get; set; } = 80m;

    /// <summary>
    /// Display order for UI sorting (lower numbers appear first).
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<AIQuota> Quotas { get; set; } = new List<AIQuota>();
}

