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
    /// Throws UnauthorizedException if AI is disabled for the organization.
    /// </summary>
    System.Threading.Tasks.Task ThrowIfAIDisabled(int organizationId, CancellationToken ct);
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
            _logger.LogWarning("AI access denied for organization {OrganizationId} - AI is disabled", organizationId);
            throw new UnauthorizedException("AI features are currently disabled for your organization. Please contact support for assistance.");
        }
    }
}

