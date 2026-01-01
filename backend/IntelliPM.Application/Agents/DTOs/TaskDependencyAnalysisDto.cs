namespace IntelliPM.Application.Agents.DTOs;

/// <summary>
/// DTO for task dependency analysis returned by the AI agent.
/// </summary>
public class TaskDependencyAnalysisDto
{
    /// <summary>
    /// ID of the analyzed task.
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// List of direct dependencies (tasks this task depends on or tasks that depend on this task).
    /// </summary>
    public List<DirectDependencyDto> DirectDependencies { get; set; } = new();

    /// <summary>
    /// List of circular dependencies detected (if any).
    /// </summary>
    public List<List<int>> CircularDependencies { get; set; } = new();

    /// <summary>
    /// Critical path for the project (list of task IDs).
    /// </summary>
    public List<int> CriticalPath { get; set; } = new();

    /// <summary>
    /// Whether the task is on the critical path.
    /// </summary>
    public bool IsOnCriticalPath { get; set; }

    /// <summary>
    /// Bottleneck risk level: "low", "medium", "high".
    /// </summary>
    public string BottleneckRisk { get; set; } = "low";

    /// <summary>
    /// List of recommendations for managing dependencies.
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// DTO for a direct dependency relationship.
/// </summary>
public class DirectDependencyDto
{
    /// <summary>
    /// Task ID involved in the dependency.
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Type of dependency: "blocks", "blocked_by", "depends_on".
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Task title.
    /// </summary>
    public string? TaskTitle { get; set; }
}

