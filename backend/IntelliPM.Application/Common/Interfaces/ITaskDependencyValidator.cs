using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for validating task dependencies and detecting cycles in the dependency graph.
/// </summary>
public interface ITaskDependencyValidator
{
    /// <summary>
    /// Checks if adding a dependency from sourceTaskId to dependentTaskId would create a cycle in the dependency graph.
    /// Uses depth-first search (DFS) algorithm to detect cycles.
    /// </summary>
    /// <param name="sourceTaskId">The task that would depend on another task</param>
    /// <param name="dependentTaskId">The task that would be depended upon</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if adding the dependency would create a cycle, false otherwise</returns>
    Task<bool> WouldCreateCycleAsync(
        int sourceTaskId,
        int dependentTaskId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the full dependency chain for a given task.
    /// Returns all task IDs that the given task depends on (directly or indirectly).
    /// </summary>
    /// <param name="taskId">The task ID to get dependencies for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of task IDs in the dependency chain (ordered from immediate to transitive dependencies)</returns>
    Task<List<int>> GetDependencyChainAsync(
        int taskId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates if a dependency type makes logical sense for the given source and dependent tasks.
    /// For example, Finish-to-Start is the most common and always valid.
    /// </summary>
    /// <param name="sourceTaskId">The task that depends on another task</param>
    /// <param name="dependentTaskId">The task being depended upon</param>
    /// <param name="dependencyType">The type of dependency to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the dependency type is valid, false otherwise</returns>
    Task<bool> ValidateDependencyTypeAsync(
        int sourceTaskId,
        int dependentTaskId,
        DependencyType dependencyType,
        CancellationToken cancellationToken);
}

