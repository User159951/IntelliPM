using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Organization-level AI quota configuration.
/// Defines base limits and settings for an organization.
/// One row per organization (unique OrganizationId).
/// </summary>
public class OrganizationAIQuota : IAggregateRoot
{
    public int Id { get; set; }
    
    /// <summary>
    /// Organization ID (unique - one quota per organization).
    /// </summary>
    public int OrganizationId { get; set; }
    
    /// <summary>
    /// Monthly token limit for the organization.
    /// </summary>
    public long MonthlyTokenLimit { get; set; }
    
    /// <summary>
    /// Monthly request limit for the organization (optional).
    /// </summary>
    public int? MonthlyRequestLimit { get; set; }
    
    /// <summary>
    /// Day of month when quota resets (1-31).
    /// If null, quota resets on the first day of each month.
    /// </summary>
    public int? ResetDayOfMonth { get; set; }
    
    /// <summary>
    /// Organization-level AI kill switch.
    /// If false, AI features are disabled for all users in this organization.
    /// </summary>
    public bool IsAIEnabled { get; set; } = true;
    
    /// <summary>
    /// Timestamp when this quota configuration was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Timestamp when this quota configuration was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
}

