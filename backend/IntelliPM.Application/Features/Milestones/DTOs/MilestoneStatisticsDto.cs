namespace IntelliPM.Application.Features.Milestones.DTOs;

/// <summary>
/// Data Transfer Object for milestone statistics.
/// Contains aggregated statistics about milestones for a project.
/// </summary>
public class MilestoneStatisticsDto
{
    /// <summary>
    /// Total number of milestones in the project.
    /// </summary>
    public int TotalMilestones { get; set; }

    /// <summary>
    /// Number of completed milestones.
    /// </summary>
    public int CompletedMilestones { get; set; }

    /// <summary>
    /// Number of missed milestones (past due date without completion).
    /// </summary>
    public int MissedMilestones { get; set; }

    /// <summary>
    /// Number of upcoming milestones (Pending or InProgress with future due date).
    /// </summary>
    public int UpcomingMilestones { get; set; }

    /// <summary>
    /// Number of pending milestones.
    /// </summary>
    public int PendingMilestones { get; set; }

    /// <summary>
    /// Number of milestones currently in progress.
    /// </summary>
    public int InProgressMilestones { get; set; }

    /// <summary>
    /// Completion rate as a percentage (0-100).
    /// Calculated as (CompletedMilestones / TotalMilestones) * 100.
    /// </summary>
    public double CompletionRate { get; set; }
}

