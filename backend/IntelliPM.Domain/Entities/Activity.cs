using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Tracks user activities across projects for activity feed
/// </summary>
public class Activity : IAggregateRoot, ITenantEntity
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty; // task_created, task_updated, sprint_started, etc.
    public string EntityType { get; set; } = string.Empty; // task, sprint, project, etc.
    public int EntityId { get; set; }
    public string? EntityName { get; set; } // Cached name for quick display
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; } // Cached project name
    public string? Metadata { get; set; } // JSON for additional context (old value, new value, etc.)
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}
