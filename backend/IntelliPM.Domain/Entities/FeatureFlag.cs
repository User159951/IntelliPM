using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Represents a feature flag for toggling features on or off.
/// Feature flags can be global (OrganizationId = null) or organization-specific.
/// </summary>
public class FeatureFlag : IAggregateRoot
{
    /// <summary>
    /// Unique identifier for the feature flag.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Unique name of the feature flag (e.g., "EnableAIInsights", "EnableAdvancedMetrics").
    /// Must be unique across all feature flags.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Indicates whether the feature is currently enabled.
    /// Default is false (disabled).
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Organization ID for organization-specific feature flags.
    /// Null indicates a global feature flag that applies to all organizations.
    /// </summary>
    public int? OrganizationId { get; private set; }

    /// <summary>
    /// Optional description explaining what the feature flag controls.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// The date and time when the feature flag was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// The date and time when the feature flag was last updated.
    /// Null if the feature flag has never been updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// EF Core requires a parameterless constructor for materialization.
    /// </summary>
    private FeatureFlag()
    {
        // Required by EF Core
    }

    /// <summary>
    /// Creates a new feature flag.
    /// </summary>
    /// <param name="name">The unique name of the feature flag</param>
    /// <param name="organizationId">Optional organization ID for organization-specific flags. Null for global flags.</param>
    /// <param name="description">Optional description of the feature flag</param>
    /// <param name="isEnabled">Whether the feature is enabled by default (default: false)</param>
    /// <returns>A new FeatureFlag instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when name is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when name is whitespace</exception>
    public static FeatureFlag Create(
        string name,
        int? organizationId = null,
        string? description = null,
        bool isEnabled = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Feature flag name cannot be null, empty, or whitespace.", nameof(name));
        }

        return new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            IsEnabled = isEnabled,
            OrganizationId = organizationId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    /// <summary>
    /// Enables the feature flag.
    /// Updates the UpdatedAt timestamp.
    /// </summary>
    public void Enable()
    {
        if (IsEnabled)
        {
            return; // Already enabled, no-op
        }

        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disables the feature flag.
    /// Updates the UpdatedAt timestamp.
    /// </summary>
    public void Disable()
    {
        if (!IsEnabled)
        {
            return; // Already disabled, no-op
        }

        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the description of the feature flag.
    /// </summary>
    /// <param name="description">The new description</param>
    public void UpdateDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Indicates whether this is a global feature flag (applies to all organizations).
    /// </summary>
    public bool IsGlobal => OrganizationId == null;

    /// <summary>
    /// Indicates whether this is an organization-specific feature flag.
    /// </summary>
    public bool IsOrganizationSpecific => OrganizationId != null;
}

