using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// User-level AI quota overrides.
/// Allows per-user customization of quota limits beyond organization defaults.
/// One row per user (unique UserId).
/// Nullable override fields mean "use organization default".
/// </summary>
public class UserAIQuota : IAggregateRoot
{
    public int Id { get; set; }
    
    /// <summary>
    /// User ID (unique - one quota override per user).
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Organization ID (denormalized for fast filtering and consistency checks).
    /// Must match User.OrganizationId.
    /// </summary>
    public int OrganizationId { get; set; }
    
    /// <summary>
    /// Monthly token limit override for this user.
    /// If null, uses OrganizationAIQuota.MonthlyTokenLimit.
    /// </summary>
    public long? MonthlyTokenLimitOverride { get; set; }
    
    /// <summary>
    /// Monthly request limit override for this user.
    /// If null, uses OrganizationAIQuota.MonthlyRequestLimit.
    /// </summary>
    public int? MonthlyRequestLimitOverride { get; set; }
    
    /// <summary>
    /// User-level AI kill switch override.
    /// If null, uses OrganizationAIQuota.IsAIEnabled.
    /// If false, AI features are disabled for this user regardless of organization setting.
    /// </summary>
    public bool? IsAIEnabledOverride { get; set; }
    
    /// <summary>
    /// Timestamp when this quota override was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Timestamp when this quota override was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}

