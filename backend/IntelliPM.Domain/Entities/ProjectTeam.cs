using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Join entity representing the many-to-many relationship between Project and Team.
/// Tracks which teams are assigned to which projects.
/// </summary>
public class ProjectTeam : IAggregateRoot
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Project.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Foreign key to the Team.
    /// </summary>
    public int TeamId { get; set; }

    /// <summary>
    /// Organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; set; }

    /// <summary>
    /// Timestamp when the team was assigned to the project.
    /// </summary>
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional foreign key to the User who assigned the team.
    /// </summary>
    public int? AssignedById { get; set; }

    /// <summary>
    /// Indicates if the team assignment is active.
    /// Can be deactivated without deletion for audit purposes.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional timestamp when the team was unassigned from the project.
    /// </summary>
    public DateTimeOffset? UnassignedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Navigation property to the Project.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Navigation property to the Team.
    /// </summary>
    public Team Team { get; set; } = null!;

    /// <summary>
    /// Navigation property to the User who assigned the team.
    /// </summary>
    public User? AssignedBy { get; set; }
}

