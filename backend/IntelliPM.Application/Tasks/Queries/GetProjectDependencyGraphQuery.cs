using IntelliPM.Application.Tasks.DTOs;
using MediatR;

namespace IntelliPM.Application.Tasks.Queries;

/// <summary>
/// Query to get the complete dependency graph for a project.
/// Returns all tasks and their dependencies for visualization.
/// </summary>
public record GetProjectDependencyGraphQuery : IRequest<DependencyGraphDto>
{
    /// <summary>
    /// The project ID to get the dependency graph for.
    /// </summary>
    public int ProjectId { get; init; }
}

