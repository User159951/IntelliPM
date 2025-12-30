using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Service for checking feature flag status with in-memory caching.
/// Caches feature flags for 5 minutes to reduce database queries.
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FeatureFlagService> _logger;
    private const int CacheExpirationMinutes = 5;
    private const string CacheKeyPrefix = "FeatureFlag_";

    public FeatureFlagService(
        AppDbContext context,
        IMemoryCache cache,
        ILogger<FeatureFlagService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks if a feature flag is enabled.
    /// First checks cache, then database if not found in cache.
    /// </summary>
    public async Task<bool> IsEnabledAsync(string featureName, int? organizationId = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            _logger.LogWarning("Feature flag name is null or empty");
            return false;
        }

        var featureFlag = await GetAsync(featureName, organizationId);
        return featureFlag?.IsEnabled ?? false;
    }

    /// <summary>
    /// Gets a feature flag by name.
    /// First checks cache, then database if not found in cache.
    /// </summary>
    public async Task<FeatureFlag?> GetAsync(string featureName, int? organizationId = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            _logger.LogWarning("Feature flag name is null or empty");
            return null;
        }

        var cacheKey = GetCacheKey(featureName, organizationId);

        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out FeatureFlag? cachedFlag))
        {
            _logger.LogDebug(
                "Feature flag {FeatureName} for OrganizationId {OrganizationId} found in cache. Enabled: {IsEnabled}",
                featureName,
                organizationId?.ToString() ?? "Global",
                cachedFlag?.IsEnabled ?? false);
            return cachedFlag;
        }

        // Not in cache, query database
        _logger.LogDebug(
            "Feature flag {FeatureName} for OrganizationId {OrganizationId} not in cache, querying database",
            featureName,
            organizationId?.ToString() ?? "Global");

        FeatureFlag? featureFlag = null;

        if (organizationId.HasValue)
        {
            // First try to get organization-specific flag
            featureFlag = await _context.Set<FeatureFlag>()
                .FirstOrDefaultAsync(f => f.Name == featureName && f.OrganizationId == organizationId.Value);

            // If not found, fall back to global flag
            if (featureFlag == null)
            {
                featureFlag = await _context.Set<FeatureFlag>()
                    .FirstOrDefaultAsync(f => f.Name == featureName && f.OrganizationId == null);
            }
        }
        else
        {
            // Only get global flag
            featureFlag = await _context.Set<FeatureFlag>()
                .FirstOrDefaultAsync(f => f.Name == featureName && f.OrganizationId == null);
        }

        // Cache the result (even if null, to avoid repeated DB queries)
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, featureFlag, cacheOptions);

        if (featureFlag != null)
        {
            _logger.LogInformation(
                "Feature flag {FeatureName} for OrganizationId {OrganizationId} loaded from database. Enabled: {IsEnabled}",
                featureName,
                organizationId?.ToString() ?? "Global",
                featureFlag.IsEnabled);
        }
        else
        {
            _logger.LogWarning(
                "Feature flag {FeatureName} for OrganizationId {OrganizationId} not found in database",
                featureName,
                organizationId?.ToString() ?? "Global");
        }

        return featureFlag;
    }

    /// <summary>
    /// Generates a cache key for a feature flag.
    /// </summary>
    private static string GetCacheKey(string featureName, int? organizationId)
    {
        var orgKey = organizationId.HasValue ? organizationId.Value.ToString() : "Global";
        return $"{CacheKeyPrefix}{featureName}_{orgKey}";
    }
}

