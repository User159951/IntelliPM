using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Service for checking feature flag status with in-memory caching.
/// Caches feature flags for 5 minutes to reduce database queries.
/// 
/// STRICT MODE: This service operates in strict mode - all feature flags must exist in the database.
/// If a flag is not found, a FeatureFlagNotFoundException is thrown. There are no default fallback values.
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
    /// 
    /// STRICT MODE: Throws FeatureFlagNotFoundException if the flag does not exist in the database.
    /// No default values are returned.
    /// </summary>
    /// <param name="featureName">The name of the feature flag (must not be null or empty)</param>
    /// <param name="organizationId">Optional organization ID. If null, checks for global flag.</param>
    /// <returns>True if the feature is enabled, false if disabled</returns>
    /// <exception cref="ArgumentNullException">Thrown when featureName is null or empty</exception>
    /// <exception cref="FeatureFlagNotFoundException">Thrown when the feature flag is not found in the database</exception>
    public async Task<bool> IsEnabledAsync(string featureName, int? organizationId = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            _logger.LogError(
                "Feature flag name is null or empty. OrganizationId: {OrganizationId}",
                organizationId?.ToString() ?? "Global");
            throw new ArgumentException("Feature flag name cannot be null or empty.", nameof(featureName));
        }

        _logger.LogDebug(
            "Checking if feature flag '{FeatureName}' is enabled for OrganizationId {OrganizationId}",
            featureName,
            organizationId?.ToString() ?? "Global");

        // GetAsync throws FeatureFlagNotFoundException if flag doesn't exist (strict mode)
        var featureFlag = await GetAsync(featureName, organizationId);

        _logger.LogInformation(
            "Feature flag '{FeatureName}' for OrganizationId {OrganizationId} is {Status}",
            featureName,
            organizationId?.ToString() ?? "Global",
            featureFlag.IsEnabled ? "ENABLED" : "DISABLED");

        return featureFlag.IsEnabled;
    }

    /// <summary>
    /// Gets a feature flag by name.
    /// First checks cache, then database if not found in cache.
    /// 
    /// STRICT MODE: Throws FeatureFlagNotFoundException if the flag does not exist in the database.
    /// No null values are returned.
    /// </summary>
    /// <param name="featureName">The name of the feature flag (must not be null or empty)</param>
    /// <param name="organizationId">Optional organization ID. If null, gets global flag.</param>
    /// <returns>The FeatureFlag entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when featureName is null or empty</exception>
    /// <exception cref="FeatureFlagNotFoundException">Thrown when the feature flag is not found in the database</exception>
    public async Task<FeatureFlag> GetAsync(string featureName, int? organizationId = null)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            _logger.LogError(
                "Feature flag name is null or empty. OrganizationId: {OrganizationId}",
                organizationId?.ToString() ?? "Global");
            throw new ArgumentException("Feature flag name cannot be null or empty.", nameof(featureName));
        }

        var cacheKey = GetCacheKey(featureName, organizationId);

        // Try to get from cache first
        // Note: In strict mode, we only cache flags that exist (non-null values)
        if (_cache.TryGetValue(cacheKey, out FeatureFlag? cachedFlag) && cachedFlag != null)
        {
            _logger.LogDebug(
                "Feature flag '{FeatureName}' for OrganizationId {OrganizationId} found in cache. Enabled: {IsEnabled}",
                featureName,
                organizationId?.ToString() ?? "Global",
                cachedFlag.IsEnabled);
            return cachedFlag;
        }

        // Not in cache, query database
        _logger.LogDebug(
            "Feature flag '{FeatureName}' for OrganizationId {OrganizationId} not in cache, querying database",
            featureName,
            organizationId?.ToString() ?? "Global");

        FeatureFlag? featureFlag = null;

        if (organizationId.HasValue)
        {
            // First try to get organization-specific flag
            _logger.LogDebug(
                "Attempting to find organization-specific feature flag '{FeatureName}' for OrganizationId {OrganizationId}",
                featureName,
                organizationId.Value);
            
            featureFlag = await _context.Set<FeatureFlag>()
                .FirstOrDefaultAsync(f => f.Name == featureName && f.OrganizationId == organizationId.Value);

            // If not found, fall back to global flag
            if (featureFlag == null)
            {
                _logger.LogDebug(
                    "Organization-specific feature flag '{FeatureName}' not found for OrganizationId {OrganizationId}, checking global flags",
                    featureName,
                    organizationId.Value);
                
                featureFlag = await _context.Set<FeatureFlag>()
                    .FirstOrDefaultAsync(f => f.Name == featureName && f.OrganizationId == null);
            }
        }
        else
        {
            // Only get global flag
            _logger.LogDebug(
                "Attempting to find global feature flag '{FeatureName}'",
                featureName);
            
            featureFlag = await _context.Set<FeatureFlag>()
                .FirstOrDefaultAsync(f => f.Name == featureName && f.OrganizationId == null);
        }

        // Cache the result - but only if found (strict mode: don't cache nulls)
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
            Priority = CacheItemPriority.Normal
        };

        if (featureFlag != null)
        {
            _cache.Set(cacheKey, featureFlag, cacheOptions);
            _logger.LogInformation(
                "Feature flag '{FeatureName}' for OrganizationId {OrganizationId} loaded from database. Enabled: {IsEnabled}",
                featureName,
                organizationId?.ToString() ?? "Global",
                featureFlag.IsEnabled);
            return featureFlag;
        }

        // Strict mode: flag not found - throw exception
        _logger.LogError(
            "Feature flag '{FeatureName}' not found in database for OrganizationId {OrganizationId} (checked organization-specific and global flags)",
            featureName,
            organizationId?.ToString() ?? "Global");
        throw new FeatureFlagNotFoundException(featureName, organizationId);
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

