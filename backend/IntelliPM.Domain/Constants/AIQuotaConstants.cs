namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for AI quota tiers, limits, and billing.
/// </summary>
public static class AIQuotaConstants
{
    /// <summary>
    /// Quota tier names.
    /// </summary>
    public static class Tiers
    {
        public const string Free = "Free";
        public const string Pro = "Pro";
        public const string Enterprise = "Enterprise";
        public const string Custom = "Custom";
    }

    /// <summary>
    /// Default quota limits per tier (monthly).
    /// </summary>
    public static readonly Dictionary<string, QuotaLimits> DefaultLimits = new()
    {
        {
            Tiers.Free, new QuotaLimits
            {
                MaxTokensPerPeriod = 100_000,
                MaxRequestsPerPeriod = 100,
                MaxDecisionsPerPeriod = 50,
                MaxCostPerPeriod = 0m,
                AllowOverage = false
            }
        },
        {
            Tiers.Pro, new QuotaLimits
            {
                MaxTokensPerPeriod = 1_000_000,
                MaxRequestsPerPeriod = 1000,
                MaxDecisionsPerPeriod = 500,
                MaxCostPerPeriod = 100m,
                AllowOverage = true,
                OverageRate = 0.00002m // $0.02 per 1000 tokens
            }
        },
        {
            Tiers.Enterprise, new QuotaLimits
            {
                MaxTokensPerPeriod = 10_000_000,
                MaxRequestsPerPeriod = 10000,
                MaxDecisionsPerPeriod = 5000,
                MaxCostPerPeriod = 1000m,
                AllowOverage = true,
                OverageRate = 0.00001m // $0.01 per 1000 tokens
            }
        }
    };

    /// <summary>
    /// Number of days in a quota period (monthly).
    /// </summary>
    public const int QuotaPeriodDays = 30;

    /// <summary>
    /// Default alert threshold percentage.
    /// </summary>
    public const decimal DefaultAlertThreshold = 80m; // 80%

    /// <summary>
    /// Cost per token (approximate, can vary by model).
    /// </summary>
    public const decimal CostPerToken = 0.00001m; // $0.01 per 1000 tokens
}

/// <summary>
/// Quota limits for a tier.
/// </summary>
public class QuotaLimits
{
    public int MaxTokensPerPeriod { get; set; }
    public int MaxRequestsPerPeriod { get; set; }
    public int MaxDecisionsPerPeriod { get; set; }
    public decimal MaxCostPerPeriod { get; set; }
    public bool AllowOverage { get; set; }
    public decimal OverageRate { get; set; }
}

