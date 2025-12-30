namespace IntelliPM.Application.Features.Milestones.DTOs;

/// <summary>
/// Data Transfer Object for Milestone entity.
/// Contains milestone information with calculated properties for display.
/// </summary>
public class MilestoneDto
{
    /// <summary>
    /// Unique identifier for the milestone.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the project this milestone belongs to.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Name of the milestone.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the milestone.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of milestone as string: "Release", "Sprint", "Deadline", "Custom".
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Status of milestone as string: "Pending", "InProgress", "Completed", "Missed", "Cancelled".
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Due date for the milestone.
    /// </summary>
    public DateTimeOffset DueDate { get; set; }

    /// <summary>
    /// Date and time when the milestone was completed.
    /// Null if not completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Number of days until the milestone is due.
    /// Negative if overdue.
    /// </summary>
    public int DaysUntilDue { get; set; }

    /// <summary>
    /// Indicates whether the milestone is overdue.
    /// True if due date has passed and milestone is not completed or cancelled.
    /// </summary>
    public bool IsOverdue { get; set; }

    /// <summary>
    /// Date and time when the milestone was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Name of the user who created the milestone.
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;
}

