using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Milestone entity representing important project milestones, deadlines, and checkpoints.
/// Milestones help track project progress and mark significant achievements or deadlines.
/// </summary>
public class Milestone : IAggregateRoot
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
    /// Name of the milestone (e.g., "Sprint 1 Complete", "Beta Release", "Project Launch").
    /// Maximum length: 200 characters.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description providing additional details about the milestone.
    /// Maximum length: 1000 characters.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of milestone (Release, Sprint, Deadline, Custom).
    /// </summary>
    public MilestoneType Type { get; set; } = MilestoneType.Custom;

    /// <summary>
    /// Current status of the milestone (Pending, InProgress, Completed, Missed, Cancelled).
    /// Default: Pending.
    /// </summary>
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    /// <summary>
    /// Due date for the milestone. This is the target date when the milestone should be reached.
    /// Required field.
    /// </summary>
    public DateTimeOffset DueDate { get; set; }

    /// <summary>
    /// Date and time when the milestone was actually completed.
    /// Null if the milestone has not been completed yet.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Progress percentage indicating how close the milestone is to completion.
    /// Range: 0-100. Default: 0.
    /// </summary>
    public int Progress { get; set; } = MilestoneConstants.DefaultProgress;

    /// <summary>
    /// Organization ID for multi-tenancy isolation.
    /// </summary>
    public int OrganizationId { get; set; }

    /// <summary>
    /// Date and time when the milestone was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Date and time when the milestone was last updated.
    /// Null if the milestone has never been updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// ID of the user who created the milestone.
    /// </summary>
    public int CreatedById { get; set; }

    // Navigation properties

    /// <summary>
    /// Navigation property to the project this milestone belongs to.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Navigation property to the user who created the milestone.
    /// </summary>
    public User CreatedBy { get; set; } = null!;

    // Domain methods for business logic

    /// <summary>
    /// Marks the milestone as completed.
    /// </summary>
    /// <param name="completedAt">The date and time when the milestone was completed.</param>
    /// <exception cref="InvalidOperationException">Thrown when completion date is invalid or milestone cannot be completed.</exception>
    public void MarkAsCompleted(DateTimeOffset completedAt)
    {
        if (completedAt > DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Completion date cannot be in the future");

        if (completedAt < CreatedAt)
            throw new InvalidOperationException("Completion date cannot be before creation date");

        if (Status == MilestoneStatus.Completed)
            throw new InvalidOperationException("Milestone is already completed");

        if (Status == MilestoneStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled milestone");

        Status = MilestoneStatus.Completed;
        CompletedAt = completedAt;
        Progress = 100;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the milestone as missed (past due date without completion).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when milestone cannot be marked as missed.</exception>
    public void MarkAsMissed()
    {
        if (DueDate > DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Cannot mark as missed before due date");

        if (Status == MilestoneStatus.Completed)
            throw new InvalidOperationException("Cannot mark completed milestone as missed");

        if (Status == MilestoneStatus.Cancelled)
            throw new InvalidOperationException("Cannot mark cancelled milestone as missed");

        Status = MilestoneStatus.Missed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the progress percentage (0-100).
    /// Automatically updates status based on progress.
    /// </summary>
    /// <param name="progress">The progress percentage (0-100).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when progress is outside valid range.</exception>
    /// <exception cref="InvalidOperationException">Thrown when progress cannot be updated for current status.</exception>
    public void UpdateProgress(int progress)
    {
        if (progress < MilestoneConstants.MinProgress || progress > MilestoneConstants.MaxProgress)
            throw new ArgumentOutOfRangeException(
                nameof(progress),
                $"Progress must be between {MilestoneConstants.MinProgress} and {MilestoneConstants.MaxProgress}");

        if (Status == MilestoneStatus.Completed)
            throw new InvalidOperationException("Cannot update progress of completed milestone");

        if (Status == MilestoneStatus.Cancelled)
            throw new InvalidOperationException("Cannot update progress of cancelled milestone");

        Progress = progress;
        UpdatedAt = DateTimeOffset.UtcNow;

        // Automatically set to InProgress if progress > 0
        if (progress > 0 && Status == MilestoneStatus.Pending)
        {
            Status = MilestoneStatus.InProgress;
        }

        // Automatically complete if progress reaches 100
        if (progress == 100 && Status == MilestoneStatus.InProgress)
        {
            Status = MilestoneStatus.Completed;
            CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// Cancels the milestone.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when milestone cannot be cancelled.</exception>
    public void Cancel()
    {
        if (Status == MilestoneStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed milestone");

        if (Status == MilestoneStatus.Cancelled)
            throw new InvalidOperationException("Milestone is already cancelled");

        Status = MilestoneStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Checks if the milestone is overdue.
    /// A milestone is overdue if its due date has passed and it's not completed or cancelled.
    /// </summary>
    /// <returns>True if the milestone is overdue, false otherwise.</returns>
    public bool IsOverdue()
    {
        return DueDate < DateTimeOffset.UtcNow
            && Status != MilestoneStatus.Completed
            && Status != MilestoneStatus.Cancelled;
    }
}

