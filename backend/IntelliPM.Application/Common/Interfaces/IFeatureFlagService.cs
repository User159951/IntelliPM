using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for checking feature flag status.
/// Supports both global and organization-specific feature flags.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag</param>
    /// <param name="organizationId">Optional organization ID. If null, checks for global flag.</param>
    /// <returns>True if the feature is enabled, false otherwise</returns>
    Task<bool> IsEnabledAsync(string featureName, int? organizationId = null);

    /// <summary>
    /// Gets a feature flag by name.
    /// </summary>
    /// <param name="featureName">The name of the feature flag</param>
    /// <param name="organizationId">Optional organization ID. If null, gets global flag.</param>
    /// <returns>The FeatureFlag entity if found, null otherwise</returns>
    Task<FeatureFlag?> GetAsync(string featureName, int? organizationId = null);
}

