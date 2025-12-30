namespace IntelliPM.Application.Tasks.DTOs;

/// <summary>
/// Data Transfer Object for project dependency graph.
/// Contains nodes (tasks) and edges (dependencies) for visualization.
/// </summary>
public record DependencyGraphDto(
    List<DependencyGraphNodeDto> Nodes,
    List<DependencyGraphEdgeDto> Edges
);

/// <summary>
/// Data Transfer Object for a node in the dependency graph (represents a task).
/// </summary>
public record DependencyGraphNodeDto(
    int TaskId,
    string Title,
    string Status,
    int? AssigneeId,
    string? AssigneeName
);

/// <summary>
/// Data Transfer Object for an edge in the dependency graph (represents a dependency).
/// </summary>
public record DependencyGraphEdgeDto(
    int Id,
    int SourceTaskId,
    int DependentTaskId,
    string DependencyType, // "FinishToStart", "StartToStart", "FinishToFinish", "StartToFinish"
    string Label // Short label like "FS", "SS", "FF", "SF"
);

