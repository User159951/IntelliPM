using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for validating task dependencies and detecting cycles in the dependency graph.
/// Implements depth-first search (DFS) algorithm to detect cycles.
/// </summary>
public class TaskDependencyValidator : ITaskDependencyValidator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskDependencyValidator> _logger;

    public TaskDependencyValidator(
        IUnitOfWork unitOfWork,
        ILogger<TaskDependencyValidator> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks if adding a dependency from sourceTaskId to dependentTaskId would create a cycle in the dependency graph.
    /// Uses depth-first search (DFS) algorithm to detect cycles.
    /// 
    /// Algorithm:
    /// 1. Build the dependency graph from existing dependencies
    /// 2. Check if there's a path from dependentTaskId back to sourceTaskId
    /// 3. If such a path exists, adding the dependency would create a cycle
    /// 
    /// Example cycle: Task A depends on Task B, Task B depends on Task C, Task C depends on Task A
    /// If we try to add: Task A -> Task B, we check if Task B can reach Task A through existing dependencies.
    /// </summary>
    public async Task<bool> WouldCreateCycleAsync(
        int sourceTaskId,
        int dependentTaskId,
        CancellationToken cancellationToken)
    {
        // A task cannot depend on itself
        if (sourceTaskId == dependentTaskId)
        {
            _logger.LogWarning(
                "Task {SourceTaskId} cannot depend on itself",
                sourceTaskId);
            return true; // Self-dependency is considered a cycle
        }

        // Build the dependency graph
        // Key: taskId, Value: list of tasks that this task depends on (DependentTaskId)
        var dependencyGraph = await BuildDependencyGraphAsync(cancellationToken);

        // Check if there's a path from dependentTaskId back to sourceTaskId
        // If dependentTaskId can reach sourceTaskId through existing dependencies,
        // then adding sourceTaskId -> dependentTaskId would create a cycle
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();

        var hasCycle = HasCycleDFS(
            dependentTaskId,
            sourceTaskId,
            dependencyGraph,
            visited,
            recursionStack);

        if (hasCycle)
        {
            _logger.LogWarning(
                "Adding dependency from Task {SourceTaskId} to Task {DependentTaskId} would create a cycle",
                sourceTaskId,
                dependentTaskId);
        }

        return hasCycle;
    }

    /// <summary>
    /// Gets the full dependency chain for a given task.
    /// Returns all task IDs that the given task depends on (directly or indirectly).
    /// Uses depth-first search to traverse the dependency graph.
    /// </summary>
    public async Task<List<int>> GetDependencyChainAsync(
        int taskId,
        CancellationToken cancellationToken)
    {
        var dependencyGraph = await BuildDependencyGraphAsync(cancellationToken);

        if (!dependencyGraph.ContainsKey(taskId))
        {
            // Task has no dependencies
            return new List<int>();
        }

        var visited = new HashSet<int>();
        var chain = new List<int>();

        // DFS to collect all dependencies
        CollectDependenciesDFS(taskId, dependencyGraph, visited, chain);

        return chain;
    }

    /// <summary>
    /// Validates if a dependency type makes logical sense for the given source and dependent tasks.
    /// Currently, all dependency types are considered valid, but this method can be extended
    /// to add business logic validation (e.g., certain types may not make sense in specific contexts).
    /// </summary>
    public async Task<bool> ValidateDependencyTypeAsync(
        int sourceTaskId,
        int dependentTaskId,
        DependencyType dependencyType,
        CancellationToken cancellationToken)
    {
        // Verify both tasks exist
        var sourceTask = await _unitOfWork.Repository<ProjectTask>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == sourceTaskId, cancellationToken);

        if (sourceTask == null)
        {
            _logger.LogWarning("Source task {SourceTaskId} not found", sourceTaskId);
            return false;
        }

        var dependentTask = await _unitOfWork.Repository<ProjectTask>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == dependentTaskId, cancellationToken);

        if (dependentTask == null)
        {
            _logger.LogWarning("Dependent task {DependentTaskId} not found", dependentTaskId);
            return false;
        }

        // Verify both tasks belong to the same organization
        if (sourceTask.OrganizationId != dependentTask.OrganizationId)
        {
            _logger.LogWarning(
                "Tasks {SourceTaskId} and {DependentTaskId} belong to different organizations",
                sourceTaskId,
                dependentTaskId);
            return false;
        }

        // Verify both tasks belong to the same project (optional business rule)
        // This can be relaxed if cross-project dependencies are allowed
        if (sourceTask.ProjectId != dependentTask.ProjectId)
        {
            _logger.LogWarning(
                "Tasks {SourceTaskId} and {DependentTaskId} belong to different projects",
                sourceTaskId,
                dependentTaskId);
            // For now, we allow cross-project dependencies, but log a warning
            // Uncomment the return false if cross-project dependencies should be disallowed
            // return false;
        }

        // All dependency types are valid
        // Future enhancements could add type-specific validation:
        // - FinishToStart: Most common, always valid
        // - StartToStart: Valid if tasks can start simultaneously
        // - FinishToFinish: Valid if tasks must finish together
        // - StartToFinish: Rare, but valid in specific scenarios

        return true;
    }

    /// <summary>
    /// Builds the dependency graph from all existing task dependencies.
    /// Returns a dictionary where:
    /// - Key: SourceTaskId (the task that depends on another)
    /// - Value: List of DependentTaskIds (tasks that the source task depends on)
    /// </summary>
    private async Task<Dictionary<int, List<int>>> BuildDependencyGraphAsync(
        CancellationToken cancellationToken)
    {
        var dependencies = await _unitOfWork.Repository<TaskDependency>()
            .Query()
            .AsNoTracking()
            .Select(d => new { d.SourceTaskId, d.DependentTaskId })
            .ToListAsync(cancellationToken);

        var graph = new Dictionary<int, List<int>>();

        foreach (var dependency in dependencies)
        {
            if (!graph.ContainsKey(dependency.SourceTaskId))
            {
                graph[dependency.SourceTaskId] = new List<int>();
            }

            graph[dependency.SourceTaskId].Add(dependency.DependentTaskId);
        }

        return graph;
    }

    /// <summary>
    /// Depth-first search to detect if there's a path from currentTaskId to targetTaskId.
    /// This is used to check if adding a dependency would create a cycle.
    /// </summary>
    /// <param name="currentTaskId">Current task being visited</param>
    /// <param name="targetTaskId">Target task we're looking for</param>
    /// <param name="graph">Dependency graph</param>
    /// <param name="visited">Set of visited nodes (to avoid revisiting)</param>
    /// <param name="recursionStack">Set of nodes in current recursion path (for cycle detection)</param>
    /// <returns>True if a path exists from currentTaskId to targetTaskId, false otherwise</returns>
    private bool HasCycleDFS(
        int currentTaskId,
        int targetTaskId,
        Dictionary<int, List<int>> graph,
        HashSet<int> visited,
        HashSet<int> recursionStack)
    {
        // If we've reached the target, we found a path (cycle would be created)
        if (currentTaskId == targetTaskId)
        {
            return true;
        }

        // Mark current node as visited and add to recursion stack
        visited.Add(currentTaskId);
        recursionStack.Add(currentTaskId);

        // If current task has dependencies, check them
        if (graph.ContainsKey(currentTaskId))
        {
            foreach (var dependentTaskId in graph[currentTaskId])
            {
                // If we haven't visited this dependent task yet, recurse
                if (!visited.Contains(dependentTaskId))
                {
                    if (HasCycleDFS(dependentTaskId, targetTaskId, graph, visited, recursionStack))
                    {
                        return true; // Path found (cycle would be created)
                    }
                }
                // If dependent task is already in recursion stack, we found a cycle in the graph
                // But we still need to check if it leads to our target
                else if (recursionStack.Contains(dependentTaskId))
                {
                    // This indicates a cycle exists, but we need to check if it includes our target
                    // For simplicity, if we're in a cycle and haven't found the target yet,
                    // we continue searching (but this is a rare edge case)
                    continue;
                }
            }
        }

        // Remove from recursion stack before backtracking
        recursionStack.Remove(currentTaskId);

        return false;
    }

    /// <summary>
    /// Collects all dependencies for a task using depth-first search.
    /// </summary>
    private void CollectDependenciesDFS(
        int taskId,
        Dictionary<int, List<int>> graph,
        HashSet<int> visited,
        List<int> chain)
    {
        if (visited.Contains(taskId))
        {
            return; // Already visited
        }

        visited.Add(taskId);

        if (graph.ContainsKey(taskId))
        {
            foreach (var dependentTaskId in graph[taskId])
            {
                // Add to chain (direct dependency)
                if (!chain.Contains(dependentTaskId))
                {
                    chain.Add(dependentTaskId);
                }

                // Recurse to get transitive dependencies
                CollectDependenciesDFS(dependentTaskId, graph, visited, chain);
            }
        }
    }
}

