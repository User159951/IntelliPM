using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for checking if AI features are enabled for an organization.
/// Provides caching for performance and throws exceptions when AI is disabled.
/// </summary>
public interface IAIAvailabilityService
{
    /// <summary>
    /// Checks if AI is enabled for an organization.
    /// </summary>
    System.Threading.Tasks.Task<bool> IsAIEnabledForOrganization(int organizationId, CancellationToken ct);

    /// <summary>
    /// Throws AIDisabledException if AI is disabled for the organization.
    /// </summary>
    System.Threading.Tasks.Task ThrowIfAIDisabled(int organizationId, CancellationToken ct);

    /// <summary>
    /// Checks if a specific quota type has been exceeded and throws AIQuotaExceededException if so.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="quotaType">Type of quota to check: "Requests", "Tokens", or "Decisions"</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AIDisabledException">Thrown if AI is disabled for the organization</exception>
    /// <exception cref="AIQuotaExceededException">Thrown if the quota has been exceeded</exception>
    System.Threading.Tasks.Task CheckQuotaAsync(int organizationId, string quotaType, CancellationToken ct);
}

/// <summary>
/// Implementation of IAIAvailabilityService with caching support.
/// </summary>
public class AIAvailabilityService : IAIAvailabilityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AIAvailabilityService> _logger;
    private const int CacheExpirationMinutes = 5;

    public AIAvailabilityService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<AIAvailabilityService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<bool> IsAIEnabledForOrganization(int organizationId, CancellationToken ct)
    {
        var cacheKey = $"ai_enabled_org_{organizationId}";

        // Check cache first
        if (_cache.TryGetValue<bool>(cacheKey, out var cachedValue))
        {
            return cachedValue;
        }

        // Check if organization has disabled quota
        var disabledQuota = await _unitOfWork.Repository<AIQuota>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.OrganizationId == organizationId && q.TierName == "Disabled" && q.IsActive, ct);

        var isEnabled = disabledQuota == null;

        // Cache result for 5 minutes
        _cache.Set(cacheKey, isEnabled, TimeSpan.FromMinutes(CacheExpirationMinutes));

        return isEnabled;
    }

    public async System.Threading.Tasks.Task ThrowIfAIDisabled(int organizationId, CancellationToken ct)
    {
        var isEnabled = await IsAIEnabledForOrganization(organizationId, ct);

        if (!isEnabled)
        {
            // Try to get the disabled quota to extract reason
            var disabledQuota = await _unitOfWork.Repository<AIQuota>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.OrganizationId == organizationId && q.TierName == "Disabled" && q.IsActive, ct);

            var reason = disabledQuota?.QuotaExceededReason ?? "AI has been disabled for your organization. Please contact support.";

            _logger.LogWarning(
                "AI access denied for organization {OrganizationId} - AI is disabled. Reason: {Reason}",
                organizationId, reason);

            throw new AIDisabledException(
                "AI features are currently disabled for your organization. Please contact an administrator for assistance.",
                organizationId,
                reason);
        }
    }

    public async System.Threading.Tasks.Task CheckQuotaAsync(int organizationId, string quotaType, CancellationToken ct)
    {
        // First check if AI is enabled
        await ThrowIfAIDisabled(organizationId, ct);

        // Get active quota
        var quota = await _unitOfWork.Repository<AIQuota>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.OrganizationId == organizationId && q.IsActive && q.TierName != "Disabled", ct);

        if (quota == null)
        {
            _logger.LogWarning("No active AI quota found for organization {OrganizationId}", organizationId);
            throw new AIDisabledException(
                "No active AI quota found for your organization. Please contact support.",
                organizationId,
                "No quota configured");
        }

        // Check if quota is enforced
        if (!quota.EnforceQuota)
        {
            return; // Quota not enforced, allow the request
        }

        // Check quota based on type
        bool isExceeded = false;
        int currentUsage = 0;
        int maxLimit = 0;

        switch (quotaType.ToLowerInvariant())
        {
            case "requests":
                currentUsage = quota.RequestsUsed;
                maxLimit = quota.MaxRequestsPerPeriod;
                isExceeded = quota.RequestsUsed >= quota.MaxRequestsPerPeriod;
                break;

            case "tokens":
                currentUsage = quota.TokensUsed;
                maxLimit = quota.MaxTokensPerPeriod;
                isExceeded = quota.TokensUsed >= quota.MaxTokensPerPeriod;
                break;

            case "decisions":
                currentUsage = quota.DecisionsMade;
                maxLimit = quota.MaxDecisionsPerPeriod;
                isExceeded = quota.DecisionsMade >= quota.MaxDecisionsPerPeriod;
                break;

            default:
                _logger.LogWarning("Unknown quota type: {QuotaType} for organization {OrganizationId}", quotaType, organizationId);
                return; // Unknown quota type, allow the request
        }

        if (isExceeded)
        {
            var quotaTypeDisplayName = quotaType switch
            {
                "requests" => "Monthly AI request limit",
                "tokens" => "Monthly AI token limit",
                "decisions" => "Monthly AI decision limit",
                _ => $"AI {quotaType} limit"
            };

            _logger.LogWarning(
                "AI quota exceeded for organization {OrganizationId}. Type: {QuotaType}, Current: {CurrentUsage}, Limit: {MaxLimit}, Tier: {TierName}",
                organizationId, quotaType, currentUsage, maxLimit, quota.TierName);

            throw new AIQuotaExceededException(
                $"{quotaTypeDisplayName} exceeded ({currentUsage}/{maxLimit}). Please upgrade to continue using AI features.",
                organizationId,
                quotaType,
                currentUsage,
                maxLimit,
                quota.TierName);
        }

        // Check if quota is approaching limit (for logging/warning purposes)
        var usagePercentage = maxLimit > 0 ? (double)currentUsage / maxLimit * 100 : 0;
        if (usagePercentage >= 80 && usagePercentage < 100)
        {
            _logger.LogInformation(
                "AI quota approaching limit for organization {OrganizationId}. Type: {QuotaType}, Usage: {UsagePercentage:F1}%",
                organizationId, quotaType, usagePercentage);
        }
    }
}
