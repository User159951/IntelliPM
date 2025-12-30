using IntelliPM.Application.Tasks.DTOs;
using IntelliPM.Domain.Enums;
using MediatR;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Command to add a dependency between two tasks.
/// </summary>
public record AddTaskDependencyCommand : IRequest<TaskDependencyDto>
{
    /// <summary>
    /// The task that depends on another task (the task that cannot proceed until the dependent task is completed/started).
    /// </summary>
    public int SourceTaskId { get; init; }

    /// <summary>
    /// The task being depended upon (the task that must be completed/started before the source task can proceed).
    /// </summary>
    public int DependentTaskId { get; init; }

    /// <summary>
    /// Type of dependency relationship (Finish-to-Start, Start-to-Start, Finish-to-Finish, Start-to-Finish).
    /// </summary>
    public DependencyType DependencyType { get; init; } = DependencyType.FinishToStart;
}

