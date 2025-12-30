namespace IntelliPM.Application.Tasks.DTOs;

/// <summary>
/// Data Transfer Object for task dependencies.
/// </summary>
public record TaskDependencyDto(
    int Id,
    int SourceTaskId,
    string SourceTaskTitle,
    int DependentTaskId,
    string DependentTaskTitle,
    string DependencyType, // "FinishToStart", "StartToStart", "FinishToFinish", "StartToFinish"
    DateTimeOffset CreatedAt,
    string CreatedByName
);

