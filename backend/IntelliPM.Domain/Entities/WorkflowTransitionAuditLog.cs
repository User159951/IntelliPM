using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Audit log for workflow transition attempts (both successful and failed).
/// Tracks who attempted transitions, from/to statuses, and whether they were allowed or denied.
/// </summary>
public class WorkflowTransitionAuditLog : IAggregateRoot
{
    public int Id { get; set; }

    /// <summary>
    /// ID of the user who attempted the transition.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Type of entity (Task, Sprint, Release).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity that was transitioned.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Source status before transition.
    /// </summary>
    public string FromStatus { get; set; } = string.Empty;

    /// <summary>
    /// Target status attempted.
    /// </summary>
    public string ToStatus { get; set; } = string.Empty;

    /// <summary>
    /// Whether the transition was allowed (true) or denied (false).
    /// </summary>
    public bool WasAllowed { get; set; }

    /// <summary>
    /// Reason for denial if WasAllowed is false.
    /// </summary>
    public string? DenialReason { get; set; }

    /// <summary>
    /// Role of the user who attempted the transition.
    /// </summary>
    public string UserRole { get; set; } = string.Empty;

    /// <summary>
    /// Project ID if applicable.
    /// </summary>
    public int? ProjectId { get; set; }

    /// <summary>
    /// Timestamp when the transition was attempted.
    /// </summary>
    public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Project? Project { get; set; }
}

