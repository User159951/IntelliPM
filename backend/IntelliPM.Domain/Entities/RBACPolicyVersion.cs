using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Tracks versions of RBAC policy changes for audit trail and rollback capability.
/// Each version represents a snapshot of roles, permissions, and role-permission mappings.
/// </summary>
public class RBACPolicyVersion : IAggregateRoot
{
    public int Id { get; set; }
    
    /// <summary>
    /// Version number (e.g., "1.0", "1.1", "2.0").
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of changes in this version.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when this version was applied.
    /// </summary>
    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// ID of the user who applied this version (SuperAdmin only).
    /// </summary>
    public int? AppliedByUserId { get; set; }
    
    /// <summary>
    /// JSON snapshot of all permissions at this version.
    /// </summary>
    public string PermissionsSnapshotJson { get; set; } = "[]";
    
    /// <summary>
    /// JSON snapshot of all role-permission mappings at this version.
    /// </summary>
    public string RolePermissionsSnapshotJson { get; set; } = "[]";
    
    /// <summary>
    /// Whether this version is the current active version.
    /// </summary>
    public bool IsActive { get; set; } = false;
    
    /// <summary>
    /// Optional notes about this version.
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Timestamp when this version was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation properties
    public User? AppliedByUser { get; set; }
}

