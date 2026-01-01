namespace IntelliPM.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when an organization's AI quota has been exceeded.
/// </summary>
public class AIQuotaExceededException : ApplicationException
{
    /// <summary>
    /// Organization ID that exceeded the quota.
    /// </summary>
    public int OrganizationId { get; }

    /// <summary>
    /// Type of quota that was exceeded (e.g., "Requests", "Tokens", "Decisions").
    /// </summary>
    public string QuotaType { get; }

    /// <summary>
    /// Current usage value.
    /// </summary>
    public int CurrentUsage { get; }

    /// <summary>
    /// Maximum limit for this quota type.
    /// </summary>
    public int MaxLimit { get; }

    /// <summary>
    /// Tier name of the organization's quota.
    /// </summary>
    public string TierName { get; }

    public AIQuotaExceededException(
        string message,
        int organizationId,
        string quotaType,
        int currentUsage,
        int maxLimit,
        string tierName)
        : base(message)
    {
        OrganizationId = organizationId;
        QuotaType = quotaType;
        CurrentUsage = currentUsage;
        MaxLimit = maxLimit;
        TierName = tierName;
    }
}

