using Microsoft.AspNetCore.Authorization;

namespace IntelliPM.API.Authorization;

/// <summary>
/// Authorization attribute that requires a specific feature flag to be enabled.
/// Usage: [RequireFeatureFlag("EnableAIInsights")]
/// Can be applied to controllers or actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireFeatureFlagAttribute : Attribute
{
    /// <summary>
    /// The name of the feature flag that must be enabled.
    /// </summary>
    public string FeatureFlagName { get; }

    /// <summary>
    /// Initializes a new instance of the RequireFeatureFlagAttribute.
    /// </summary>
    /// <param name="featureFlagName">The name of the feature flag that must be enabled</param>
    public RequireFeatureFlagAttribute(string featureFlagName)
    {
        if (string.IsNullOrWhiteSpace(featureFlagName))
        {
            throw new ArgumentException("Feature flag name cannot be null or empty.", nameof(featureFlagName));
        }

        FeatureFlagName = featureFlagName;
    }
}

