using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Infrastructure.Persistence;

namespace IntelliPM.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin for analyzing task dependencies:
/// retrieving dependencies, detecting cycles, and calculating critical path.
/// </summary>
public class TaskDependencyPlugin
{
    private readonly AppDbContext _context;

    public TaskDependencyPlugin(AppDbContext context)
    {
        _context = context;
    }

    [KernelFunction]
    [Description("Get a task with all its dependencies (both tasks it depends on and tasks that depend on it)")]
    public async Task<TaskWithDependenciesInfo> GetTaskWithDependencies(int taskId)
    {
        var task = await _context.ProjectTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return new TaskWithDependenciesInfo
            {
                TaskId = taskId,
                TaskTitle = "Task not found",
                HasDependencies = false
            };
        }

        // Get dependencies where this task is the source (tasks this task depends on)
        var blockingDependencies = await _context.TaskDependencies
            .AsNoTracking()
            .Where(d => d.SourceTaskId == taskId)
            .Include(d => d.DependentTask)
            .Select(d => new DependencyInfo
            {
                TaskId = d.DependentTaskId,
                TaskTitle = d.DependentTask.Title,
                DependencyType = d.DependencyType.ToString(),
                Status = d.DependentTask.Status
            })
            .ToListAsync();

        // Get dependencies where this task is the dependent (tasks that depend on this task)
        var blockingTasks = await _context.TaskDependencies
            .AsNoTracking()
            .Where(d => d.DependentTaskId == taskId)
            .Include(d => d.SourceTask)
            .Select(d => new DependencyInfo
            {
                TaskId = d.SourceTaskId,
                TaskTitle = d.SourceTask.Title,
                DependencyType = d.DependencyType.ToString(),
                Status = d.SourceTask.Status
            })
            .ToListAsync();

        return new TaskWithDependenciesInfo
        {
            TaskId = taskId,
            TaskTitle = task.Title,
            TaskStatus = task.Status,
            HasDependencies = blockingDependencies.Any() || blockingTasks.Any(),
            DependenciesOn = blockingDependencies, // Tasks this task depends on (blockers)
            BlockedByTasks = blockingTasks, // Tasks that depend on this task
            TotalDependencies = blockingDependencies.Count + blockingTasks.Count
        };
    }

    [KernelFunction]
    [Description("Get all tasks for a project with their basic information (for dependency analysis)")]
    public async Task<List<TaskInfo>> GetProjectTasks(int projectId)
    {
        return await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new TaskInfo
            {
                TaskId = t.Id,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                StoryPoints = t.StoryPoints != null ? t.StoryPoints.Value : (int?)null,
                AssigneeId = t.AssigneeId
            })
            .Take(500) // Limit to 500 tasks for performance
            .ToListAsync();
    }

    [KernelFunction]
    [Description("Detect circular dependencies starting from a task (returns list of task IDs in the cycle if found)")]
    public async Task<CircularDependencyInfo> DetectCircularDependencies(int taskId)
    {
        // Build dependency graph
        var dependencies = await _context.TaskDependencies
            .AsNoTracking()
            .ToListAsync();

        var graph = new Dictionary<int, List<int>>();
        foreach (var dep in dependencies)
        {
            if (!graph.ContainsKey(dep.SourceTaskId))
            {
                graph[dep.SourceTaskId] = new List<int>();
            }
            graph[dep.SourceTaskId].Add(dep.DependentTaskId);
        }

        // DFS to detect cycle
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();
        var cycle = new List<int>();

        bool HasCycleDFS(int node)
        {
            visited.Add(node);
            recursionStack.Add(node);

            if (graph.ContainsKey(node))
            {
                foreach (var neighbor in graph[node])
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (HasCycleDFS(neighbor))
                        {
                            cycle.Add(node);
                            return true;
                        }
                    }
                    else if (recursionStack.Contains(neighbor))
                    {
                        // Cycle detected
                        cycle.Add(node);
                        cycle.Add(neighbor);
                        return true;
                    }
                }
            }

            recursionStack.Remove(node);
            return false;
        }

        var hasCycle = HasCycleDFS(taskId);
        cycle.Reverse(); // Reverse to get correct order

        return new CircularDependencyInfo
        {
            TaskId = taskId,
            HasCircularDependency = hasCycle,
            CycleTaskIds = hasCycle ? cycle : new List<int>()
        };
    }

    [KernelFunction]
    [Description("Calculate the critical path for a project (longest path of dependent tasks) - simplified version")]
    public async Task<CriticalPathInfo> CalculateCriticalPath(int projectId)
    {
        // Get all tasks for the project
        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();

        if (!tasks.Any())
        {
            return new CriticalPathInfo
            {
                ProjectId = projectId,
                HasCriticalPath = false,
                CriticalPathTaskIds = new List<int>(),
                EstimatedDuration = 0
            };
        }

        // Get all dependencies for the project
        var taskIds = tasks.Select(t => t.Id).ToList();
        var dependencies = await _context.TaskDependencies
            .AsNoTracking()
            .Where(d => taskIds.Contains(d.SourceTaskId) && taskIds.Contains(d.DependentTaskId))
            .ToListAsync();

        // Build graph (SourceTaskId -> DependentTaskId means Source depends on Dependent)
        var graph = new Dictionary<int, List<int>>();
        var inDegree = new Dictionary<int, int>();

        foreach (var task in tasks)
        {
            inDegree[task.Id] = 0;
            graph[task.Id] = new List<int>();
        }

        foreach (var dep in dependencies)
        {
            if (!graph.ContainsKey(dep.DependentTaskId))
            {
                graph[dep.DependentTaskId] = new List<int>();
            }
            graph[dep.DependentTaskId].Add(dep.SourceTaskId);
            inDegree[dep.SourceTaskId] = inDegree.GetValueOrDefault(dep.SourceTaskId, 0) + 1;
        }

        // Topological sort with longest path (critical path algorithm)
        var queue = new Queue<int>();
        var dist = new Dictionary<int, int>();
        var prev = new Dictionary<int, int>();

        foreach (var task in tasks)
        {
            if (inDegree[task.Id] == 0)
            {
                queue.Enqueue(task.Id);
                dist[task.Id] = task.StoryPoints?.Value ?? 1; // Use story points as duration estimate
            }
            else
            {
                dist[task.Id] = 0;
            }
        }

        int maxDist = 0;
        int endTask = 0;

        while (queue.Count > 0)
        {
            var u = queue.Dequeue();

            if (graph.ContainsKey(u))
            {
                foreach (var v in graph[u])
                {
                    var task = tasks.FirstOrDefault(t => t.Id == v);
                    var duration = task?.StoryPoints?.Value ?? 1;
                    
                    if (dist[u] + duration > dist[v])
                    {
                        dist[v] = dist[u] + duration;
                        prev[v] = u;
                    }

                    inDegree[v]--;
                    if (inDegree[v] == 0)
                    {
                        queue.Enqueue(v);
                    }

                    if (dist[v] > maxDist)
                    {
                        maxDist = dist[v];
                        endTask = v;
                    }
                }
            }
        }

        // Reconstruct critical path
        var criticalPath = new List<int>();
        if (endTask > 0)
        {
            var current = endTask;
            while (current > 0 && prev.ContainsKey(current))
            {
                criticalPath.Insert(0, current);
                current = prev[current];
            }
            if (current > 0 && !criticalPath.Contains(current))
            {
                criticalPath.Insert(0, current);
            }
        }

        return new CriticalPathInfo
        {
            ProjectId = projectId,
            HasCriticalPath = criticalPath.Any(),
            CriticalPathTaskIds = criticalPath,
            EstimatedDuration = maxDist
        };
    }
}

public class TaskWithDependenciesInfo
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string? TaskStatus { get; set; }
    public bool HasDependencies { get; set; }
    public List<DependencyInfo> DependenciesOn { get; set; } = new(); // Tasks this task depends on
    public List<DependencyInfo> BlockedByTasks { get; set; } = new(); // Tasks that depend on this task
    public int TotalDependencies { get; set; }
}

public class DependencyInfo
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string DependencyType { get; set; } = string.Empty;
    public string? Status { get; set; }
}

public class TaskInfo
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public int? AssigneeId { get; set; }
}

public class CircularDependencyInfo
{
    public int TaskId { get; set; }
    public bool HasCircularDependency { get; set; }
    public List<int> CycleTaskIds { get; set; } = new();
}

public class CriticalPathInfo
{
    public int ProjectId { get; set; }
    public bool HasCriticalPath { get; set; }
    public List<int> CriticalPathTaskIds { get; set; } = new();
    public int EstimatedDuration { get; set; }
}

