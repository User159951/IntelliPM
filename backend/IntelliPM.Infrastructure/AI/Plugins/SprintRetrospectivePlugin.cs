using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using IntelliPM.Domain.Constants;
using IntelliPM.Infrastructure.Persistence;

namespace IntelliPM.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin for generating sprint retrospectives:
/// retrieving sprint metrics, completed/incomplete tasks, defects, and team activity.
/// </summary>
public class SprintRetrospectivePlugin
{
    private readonly AppDbContext _context;

    public SprintRetrospectivePlugin(AppDbContext context)
    {
        _context = context;
    }

    [KernelFunction]
    [Description("Get sprint metrics including velocity, completion rate, and story points")]
    public async Task<SprintMetricsInfo> GetSprintMetrics(int sprintId)
    {
        var sprint = await _context.Sprints
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null)
        {
            return new SprintMetricsInfo
            {
                SprintId = sprintId,
                HasData = false
            };
        }

        // Get all tasks for this sprint
        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId)
            .ToListAsync();

        var completedTasks = tasks.Where(t => t.Status == TaskConstants.Statuses.Done).ToList();
        var plannedStoryPoints = tasks.Where(t => t.StoryPoints != null).Sum(t => t.StoryPoints!.Value);
        var completedStoryPoints = completedTasks.Where(t => t.StoryPoints != null).Sum(t => t.StoryPoints!.Value);

        var completionRate = tasks.Count > 0 ? (decimal)completedTasks.Count / tasks.Count : 0;
        var velocity = completedStoryPoints;

        // Get previous sprint for comparison
        var previousSprint = await _context.Sprints
            .AsNoTracking()
            .Where(s => s.ProjectId == sprint.ProjectId 
                && s.Status == SprintConstants.Statuses.Completed 
                && s.EndDate < sprint.EndDate)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        decimal? previousVelocity = null;
        string? velocityChange = null;

        if (previousSprint != null)
        {
            var previousTasks = await _context.ProjectTasks
                .AsNoTracking()
                .Where(t => t.SprintId == previousSprint.Id && t.Status == TaskConstants.Statuses.Done && t.StoryPoints != null)
                .ToListAsync();
            
            previousVelocity = previousTasks.Sum(t => t.StoryPoints!.Value);
            
            if (previousVelocity > 0)
            {
                var change = ((decimal)velocity - previousVelocity.Value) / previousVelocity.Value * 100;
                velocityChange = change >= 0 ? $"+{change:F1}%" : $"{change:F1}%";
            }
        }

        // Get defect count for this sprint
        var defectCount = await _context.Defects
            .AsNoTracking()
            .Where(d => d.ProjectId == sprint.ProjectId 
                && d.ReportedAt >= sprint.StartDate 
                && d.ReportedAt <= sprint.EndDate)
            .CountAsync();

        return new SprintMetricsInfo
        {
            SprintId = sprintId,
            SprintNumber = sprint.Number,
            HasData = true,
            PlannedStoryPoints = plannedStoryPoints,
            CompletedStoryPoints = completedStoryPoints,
            CompletionRate = completionRate,
            Velocity = velocity,
            PreviousVelocity = previousVelocity.HasValue ? (int)previousVelocity.Value : null,
            VelocityChange = velocityChange,
            DefectCount = defectCount,
            TotalTasks = tasks.Count,
            CompletedTasks = completedTasks.Count
        };
    }

    [KernelFunction]
    [Description("Get all completed tasks for a sprint with their details")]
    public async Task<List<CompletedTaskInfo>> GetCompletedTasks(int sprintId)
    {
        return await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId && t.Status == TaskConstants.Statuses.Done)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => new CompletedTaskInfo
            {
                TaskId = t.Id,
                Title = t.Title,
                Priority = t.Priority,
                StoryPoints = t.StoryPoints != null ? t.StoryPoints.Value : (int?)null,
                AssigneeName = t.Assignee != null 
                    ? $"{t.Assignee.FirstName} {t.Assignee.LastName}" 
                    : "Unassigned",
                CompletedAt = t.UpdatedAt
            })
            .Take(100) // Limit to 100 completed tasks
            .ToListAsync();
    }

    [KernelFunction]
    [Description("Get all incomplete tasks (not done) for a sprint")]
    public async Task<List<IncompleteTaskInfo>> GetIncompleteTasks(int sprintId)
    {
        return await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId && t.Status != TaskConstants.Statuses.Done)
            .Include(t => t.Assignee)
            .OrderBy(t => t.Priority == TaskConstants.Priorities.Critical ? 1 :
                          t.Priority == TaskConstants.Priorities.High ? 2 :
                          t.Priority == TaskConstants.Priorities.Medium ? 3 : 4)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new IncompleteTaskInfo
            {
                TaskId = t.Id,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                StoryPoints = t.StoryPoints != null ? t.StoryPoints.Value : (int?)null,
                AssigneeName = t.Assignee != null 
                    ? $"{t.Assignee.FirstName} {t.Assignee.LastName}" 
                    : "Unassigned"
            })
            .Take(50) // Limit to 50 incomplete tasks
            .ToListAsync();
    }

    [KernelFunction]
    [Description("Get defects discovered during the sprint with severity and status")]
    public async Task<List<DefectInfo>> GetSprintDefects(int sprintId)
    {
        var sprint = await _context.Sprints
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null)
        {
            return new List<DefectInfo>();
        }

        return await _context.Defects
            .AsNoTracking()
            .Where(d => d.ProjectId == sprint.ProjectId 
                && d.ReportedAt >= sprint.StartDate 
                && d.ReportedAt <= sprint.EndDate)
            .OrderByDescending(d => d.Severity == "Critical" ? 4 :
                                  d.Severity == "High" ? 3 :
                                  d.Severity == "Medium" ? 2 : 1)
            .ThenByDescending(d => d.ReportedAt)
            .Select(d => new DefectInfo
            {
                DefectId = d.Id,
                Title = d.Title,
                Severity = d.Severity,
                Status = d.Status,
                ReportedAt = d.ReportedAt
            })
            .Take(50) // Limit to 50 defects
            .ToListAsync();
    }

    [KernelFunction]
    [Description("Get team activity metrics for the sprint (task updates, assignments)")]
    public async Task<TeamActivityInfo> GetTeamActivity(int sprintId)
    {
        var sprint = await _context.Sprints
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null)
        {
            return new TeamActivityInfo
            {
                SprintId = sprintId,
                HasData = false
            };
        }

        // Get task updates during sprint
        var taskUpdates = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId 
                && t.UpdatedAt >= sprint.StartDate 
                && t.UpdatedAt <= sprint.EndDate)
            .CountAsync();

        // Get unique assignees
        var assignees = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId && t.AssigneeId != null)
            .Select(t => t.AssigneeId!.Value)
            .Distinct()
            .CountAsync();

        // Get tasks with multiple status changes (indicating activity)
        var activeTasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId 
                && t.UpdatedAt >= sprint.StartDate 
                && t.UpdatedAt <= sprint.EndDate
                && t.UpdatedAt != t.CreatedAt)
            .CountAsync();

        // Calculate engagement score (simplified: ratio of active tasks to total)
        var totalTasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId)
            .CountAsync();

        var engagementLevel = totalTasks > 0 
            ? (activeTasks * 100.0m / totalTasks) > 70 ? "high" 
            : (activeTasks * 100.0m / totalTasks) > 40 ? "medium" 
            : "low"
            : "unknown";

        return new TeamActivityInfo
        {
            SprintId = sprintId,
            HasData = true,
            TaskUpdates = taskUpdates,
            ActiveTeamMembers = assignees,
            ActiveTasks = activeTasks,
            TotalTasks = totalTasks,
            EngagementLevel = engagementLevel
        };
    }
}

public class SprintMetricsInfo
{
    public int SprintId { get; set; }
    public int SprintNumber { get; set; }
    public bool HasData { get; set; }
    public int PlannedStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public decimal CompletionRate { get; set; }
    public int Velocity { get; set; }
    public int? PreviousVelocity { get; set; }
    public string? VelocityChange { get; set; }
    public int DefectCount { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
}

public class CompletedTaskInfo
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public DateTimeOffset CompletedAt { get; set; }
}

public class IncompleteTaskInfo
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
}

public class DefectInfo
{
    public int DefectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset ReportedAt { get; set; }
}

public class TeamActivityInfo
{
    public int SprintId { get; set; }
    public bool HasData { get; set; }
    public int TaskUpdates { get; set; }
    public int ActiveTeamMembers { get; set; }
    public int ActiveTasks { get; set; }
    public int TotalTasks { get; set; }
    public string EngagementLevel { get; set; } = "unknown"; // "low", "medium", "high"
}

