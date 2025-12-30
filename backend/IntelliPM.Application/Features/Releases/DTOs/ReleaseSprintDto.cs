namespace IntelliPM.Application.Features.Releases.DTOs;

/// <summary>
/// Lightweight DTO for sprint information in release context.
/// Contains essential sprint data with task completion metrics.
/// </summary>
public class ReleaseSprintDto
{
    /// <summary>
    /// Unique identifier for the sprint.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Sprint name or identifier (e.g., "Sprint 1", "Sprint 2024-01").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Sprint start date.
    /// </summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Sprint end date.
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// Sprint status as string (e.g., "NotStarted", "Active", "Completed").
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Number of completed tasks in the sprint.
    /// </summary>
    public int CompletedTasksCount { get; set; }

    /// <summary>
    /// Total number of tasks in the sprint.
    /// </summary>
    public int TotalTasksCount { get; set; }

    /// <summary>
    /// Completion percentage calculated as (CompletedTasksCount / TotalTasksCount * 100).
    /// Returns 0 if TotalTasksCount is 0.
    /// </summary>
    public int CompletionPercentage { get; set; }
}

