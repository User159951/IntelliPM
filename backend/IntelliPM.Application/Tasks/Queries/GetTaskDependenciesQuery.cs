using IntelliPM.Application.Tasks.DTOs;
using MediatR;

namespace IntelliPM.Application.Tasks.Queries;

/// <summary>
/// Query to get all dependencies for a specific task.
/// Returns dependencies where the task is either the source or the dependent task.
/// </summary>
public record GetTaskDependenciesQuery : IRequest<List<TaskDependencyDto>>
{
    /// <summary>
    /// The task ID to get dependencies for.
    /// </summary>
    public int TaskId { get; init; }
}

