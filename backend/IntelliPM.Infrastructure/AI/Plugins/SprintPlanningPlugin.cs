using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using IntelliPM.Domain.Constants;
using IntelliPM.Infrastructure.Persistence;

namespace IntelliPM.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin offering structured data for sprint planning:
/// backlog tasks, team capacity, and sprint capacity.
/// </summary>
public class SprintPlanningPlugin
{
    private readonly AppDbContext _context;

    public SprintPlanningPlugin(AppDbContext context)
    {
        _context = context;
    }

    [KernelFunction]
    [Description("Get backlog tasks available for sprint planning")]
    public async Task<List<BacklogTaskInfo>> GetBacklogTasks(int projectId)
    {
        // Backlog = project tasks not assigned to any sprint and still Todo
        return await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId
                && t.SprintId == null
                && t.Status == TaskConstants.Statuses.Todo)
            .Include(t => t.Assignee)
            .OrderByDescending(t =>
                t.Priority == TaskConstants.Priorities.Critical ? 4 :
                t.Priority == TaskConstants.Priorities.High ? 3 :
                t.Priority == TaskConstants.Priorities.Medium ? 2 : 1)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new BacklogTaskInfo
            {
                TaskId = t.Id,
                Title = t.Title,
                Priority = t.Priority,
                StoryPoints = t.StoryPoints != null ? t.StoryPoints.Value : (int?)null,
                CurrentAssignee = t.Assignee != null
                    ? $"{t.Assignee.FirstName} {t.Assignee.LastName}"
                    : null
            })
            .Take(50)
            .ToListAsync();
    }

    [KernelFunction]
    [Description("Get team member capacity and current workload for a project's active sprint")]
    public async Task<List<TeamCapacityInfo>> GetTeamCapacity(int projectId)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Members)
                .ThenInclude(pm => pm.User)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return new List<TeamCapacityInfo>();
        }

        // Find an active sprint for this project (if any)
        var activeSprint = await _context.Sprints
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId && s.Status == SprintConstants.Statuses.Active)
            .OrderBy(s => s.Number)
            .FirstOrDefaultAsync();

        var sprintId = activeSprint?.Id;

        var teamCapacity = new List<TeamCapacityInfo>();

        foreach (var member in project.Members)
        {
            var query = _context.ProjectTasks.AsNoTracking()
                .Where(t => t.AssigneeId == member.UserId && t.Status != TaskConstants.Statuses.Done);

            if (sprintId.HasValue)
            {
                query = query.Where(t => t.SprintId == sprintId.Value);
            }

            var currentWorkload = await query
                .Where(t => t.StoryPoints != null)
                .SumAsync(t => (int?)t.StoryPoints!.Value) ?? 0;

            const int defaultCapacity = 20;

            teamCapacity.Add(new TeamCapacityInfo
            {
                UserId = member.UserId,
                Name = $"{member.User.FirstName} {member.User.LastName}",
                CurrentStoryPoints = currentWorkload,
                EstimatedCapacity = defaultCapacity,
                AvailableCapacity = Math.Max(0, defaultCapacity - currentWorkload)
            });
        }

        return teamCapacity;
    }

    [KernelFunction]
    [Description("Get sprint details and remaining capacity based on assigned project tasks")]
    public async Task<SprintCapacityInfo> GetSprintCapacity(int sprintId)
    {
        var sprint = await _context.Sprints
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null)
        {
            throw new KeyNotFoundException($"Sprint {sprintId} not found");
        }

        var sprintTasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId)
            .ToListAsync();

        var assignedPoints = sprintTasks
            .Where(t => t.StoryPoints != null)
            .Sum(t => t.StoryPoints!.Value);

        // Approximate total capacity based on team capacity heuristic: members * 20 or default
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == sprint.ProjectId);

        var memberCount = project?.Members.Count ?? 0;
        var totalCapacity = memberCount > 0 ? memberCount * 20 : Math.Max(assignedPoints, 40);

        return new SprintCapacityInfo
        {
            SprintId = sprint.Id,
            SprintNumber = sprint.Number,
            TotalCapacity = totalCapacity,
            AssignedStoryPoints = assignedPoints,
            RemainingCapacity = totalCapacity - assignedPoints
        };
    }
}

public class BacklogTaskInfo
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public string? CurrentAssignee { get; set; }
}

public class TeamCapacityInfo
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CurrentStoryPoints { get; set; }
    public int EstimatedCapacity { get; set; }
    public int AvailableCapacity { get; set; }
}

public class SprintCapacityInfo
{
    public int SprintId { get; set; }
    public int SprintNumber { get; set; }
    public int TotalCapacity { get; set; }
    public int AssignedStoryPoints { get; set; }
    public int RemainingCapacity { get; set; }
}

