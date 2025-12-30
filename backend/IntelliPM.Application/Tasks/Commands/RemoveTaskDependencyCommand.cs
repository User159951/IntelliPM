using MediatR;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Command to remove a task dependency.
/// </summary>
public record RemoveTaskDependencyCommand : IRequest<Unit>
{
    /// <summary>
    /// The ID of the dependency to remove.
    /// </summary>
    public int DependencyId { get; init; }
}

