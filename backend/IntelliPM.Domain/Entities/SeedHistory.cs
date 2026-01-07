using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Tracks which seed scripts have been applied to the database.
/// Used by SeedVersionManager to ensure idempotent seeding.
/// </summary>
public class SeedHistory : IAggregateRoot
{
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the seed script (e.g., "PermissionsSeed", "RolesSeed").
    /// </summary>
    public string SeedName { get; set; } = string.Empty;
    
    /// <summary>
    /// Version of the seed script (e.g., "1.0", "1.1").
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when this seed was applied.
    /// </summary>
    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Whether the seed was applied successfully.
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Error message if the seed failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Number of records created/updated by this seed.
    /// </summary>
    public int RecordsAffected { get; set; } = 0;
    
    /// <summary>
    /// Optional description of what this seed does.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Timestamp when this record was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

