namespace IntelliPM.Application.Agents.DTOs;

/// <summary>
/// DTO for sprint plan suggestion returned by the AI agent.
/// </summary>
public class SprintPlanSuggestionDto
{
    /// <summary>
    /// List of suggested tasks to include in the sprint.
    /// </summary>
    public List<SprintTaskSuggestion> SuggestedTasks { get; set; } = new();

    /// <summary>
    /// Total story points of suggested tasks.
    /// </summary>
    public int TotalStoryPoints { get; set; }

    /// <summary>
    /// Capacity utilization percentage (0.0 to 1.0).
    /// </summary>
    public double CapacityUtilization { get; set; }

    /// <summary>
    /// List of identified risks or concerns.
    /// </summary>
    public List<string> Risks { get; set; } = new();

    /// <summary>
    /// Reasoning for the suggestion.
    /// </summary>
    public string? Reasoning { get; set; }
}

/// <summary>
/// DTO for a single task suggestion in the sprint plan.
/// </summary>
public class SprintTaskSuggestion
{
    /// <summary>
    /// Task ID from the backlog.
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Task title.
    /// </summary>
    public string TaskTitle { get; set; } = string.Empty;

    /// <summary>
    /// Suggested assignee name (optional).
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Story points for this task.
    /// </summary>
    public int? StoryPoints { get; set; }

    /// <summary>
    /// Priority level.
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Reasoning for including this task.
    /// </summary>
    public string? Reasoning { get; set; }
}

