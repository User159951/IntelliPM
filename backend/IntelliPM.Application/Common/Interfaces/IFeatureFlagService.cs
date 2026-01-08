using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for checking feature flag status.
/// Supports both global and organization-specific feature flags.
/// 
/// STRICT MODE: This service operates in strict mode - all feature flags must exist in the database.
/// If a flag is not found, a FeatureFlagNotFoundException is thrown. There are no default fallback values.
/// </summary>
public interface IFeatureFlagService
{
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
    /// <exception cref="ArgumentException">Thrown when featureName is null or empty</exception>
    /// <exception cref="FeatureFlagNotFoundException">Thrown when the feature flag is not found in the database</exception>
    Task<bool> IsEnabledAsync(string featureName, int? organizationId = null);

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
    /// <exception cref="ArgumentException">Thrown when featureName is null or empty</exception>
    /// <exception cref="FeatureFlagNotFoundException">Thrown when the feature flag is not found in the database</exception>
    Task<FeatureFlag> GetAsync(string featureName, int? organizationId = null);
}

