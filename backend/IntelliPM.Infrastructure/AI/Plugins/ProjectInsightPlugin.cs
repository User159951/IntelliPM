using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using IntelliPM.Domain.Constants;
using IntelliPM.Infrastructure.Persistence;

namespace IntelliPM.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin exposing project insight data (tasks + sprints) as tool functions.
/// </summary>
public class ProjectInsightPlugin
{
    private readonly AppDbContext _context;

    public ProjectInsightPlugin(AppDbContext context)
    {
        _context = context;
    }

    [KernelFunction]
    [Description("Get comprehensive project status including task breakdown and sprint health")]
    public async Task<ProjectStatusData> GetProjectStatus(int projectId)
    {
        // Load basic project data
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            throw new KeyNotFoundException($"Project {projectId} not found");
        }

        // Load project tasks and sprints separately (no direct navigation collection)
        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();

        var sprints = await _context.Sprints
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .ToListAsync();

        var totalTasks = tasks.Count;
        var completedTasks = tasks.Count(t => t.Status == TaskConstants.Statuses.Done);
        var inProgressTasks = tasks.Count(t => t.Status == TaskConstants.Statuses.InProgress);
        var blockedTasks = tasks.Count(t => t.Status == TaskConstants.Statuses.Blocked);
        var todoTasks = tasks.Count(t => t.Status == TaskConstants.Statuses.Todo);
        var highPriorityTasks = tasks.Count(t => t.Priority == TaskConstants.Priorities.High || t.Priority == TaskConstants.Priorities.Critical);
        var unassignedTasks = tasks.Count(t => t.AssigneeId == null);
        var activeSprints = sprints.Count(s => s.Status == SprintConstants.Statuses.Active);

        var completionPercentage = totalTasks > 0
            ? Math.Round((double)completedTasks * 100 / totalTasks, 1)
            : 0;

        return new ProjectStatusData
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            InProgressTasks = inProgressTasks,
            BlockedTasks = blockedTasks,
            TodoTasks = todoTasks,
            HighPriorityTasks = highPriorityTasks,
            UnassignedTasks = unassignedTasks,
            ActiveSprints = activeSprints,
            CompletionPercentage = completionPercentage
        };
    }

    [KernelFunction]
    [Description("Get list of blocked tasks requiring immediate attention")]
    public async Task<List<BlockedTaskInfo>> GetBlockedTasks(int projectId)
    {
        return await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId && t.Status == TaskConstants.Statuses.Blocked)
            .Include(t => t.Assignee)
            .Select(t => new BlockedTaskInfo
            {
                TaskId = t.Id,
                Title = t.Title,
                Priority = t.Priority,
                AssigneeName = t.Assignee != null
                    ? $"{t.Assignee.FirstName} {t.Assignee.LastName}"
                    : "Unassigned",
                DaysBlocked = (int)(DateTimeOffset.UtcNow - t.CreatedAt).TotalDays
            })
            .ToListAsync();
    }
}

public class ProjectStatusData
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public int TodoTasks { get; set; }
    public int HighPriorityTasks { get; set; }
    public int UnassignedTasks { get; set; }
    public int ActiveSprints { get; set; }
    public double CompletionPercentage { get; set; }
}

public class BlockedTaskInfo
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string AssigneeName { get; set; } = string.Empty;
    public int DaysBlocked { get; set; }
}

