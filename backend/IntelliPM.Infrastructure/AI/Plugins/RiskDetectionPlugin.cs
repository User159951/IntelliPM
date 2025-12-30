using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using IntelliPM.Domain.Constants;
using IntelliPM.Infrastructure.Persistence;

namespace IntelliPM.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin that exposes structured risk data for projects and sprints.
/// </summary>
public class RiskDetectionPlugin
{
    private readonly AppDbContext _context;

    public RiskDetectionPlugin(AppDbContext context)
    {
        _context = context;
    }

    [KernelFunction]
    [Description("Detect sprint capacity and deadline risks for active sprints in a project")]
    public async Task<List<SprintRiskInfo>> DetectSprintRisks(int projectId)
    {
        var risks = new List<SprintRiskInfo>();

        var activeSprints = await _context.Sprints
            .AsNoTracking()
            .Include(s => s.Project)
            .Where(s => s.ProjectId == projectId && s.Status == SprintConstants.Statuses.Active)
            .ToListAsync();

        if (!activeSprints.Any())
        {
            return risks;
        }

        // Approximate team capacity for the project based on member count * 20 default points
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        var memberCount = project?.Members.Count ?? 0;
        var defaultPerMemberCapacity = 20;
        var projectCapacity = memberCount > 0 ? memberCount * defaultPerMemberCapacity : 40; // Fallback capacity

        foreach (var sprint in activeSprints)
        {
            // Sum story points from project tasks assigned to this sprint
            var sprintTasks = await _context.ProjectTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId && t.SprintId == sprint.Id)
                .ToListAsync();

            var totalStoryPoints = sprintTasks
                .Where(t => t.StoryPoints != null)
                .Sum(t => t.StoryPoints!.Value);

            if (totalStoryPoints > projectCapacity)
            {
                risks.Add(new SprintRiskInfo
                {
                    SprintId = sprint.Id,
                    SprintNumber = sprint.Number,
                    RiskType = "OverCapacity",
                    Severity = "High",
                    Description = $"Sprint {sprint.Number} has {totalStoryPoints} points but estimated capacity is {projectCapacity}",
                    Recommendation = "Move lower priority tasks to backlog or increase team capacity"
                });
            }

            if (sprint.EndDate.HasValue)
            {
                var daysRemaining = (sprint.EndDate.Value - DateTimeOffset.UtcNow).Days;
                var incompleteTasks = sprintTasks.Count(t => t.Status != TaskConstants.Statuses.Done);

                if (daysRemaining <= 3 && incompleteTasks > 0)
                {
                    risks.Add(new SprintRiskInfo
                    {
                        SprintId = sprint.Id,
                        SprintNumber = sprint.Number,
                        RiskType = "DeadlineRisk",
                        Severity = incompleteTasks > 5 ? "High" : "Medium",
                        Description = $"Sprint ends in {daysRemaining} days with {incompleteTasks} incomplete tasks",
                        Recommendation = "Focus on completing in-progress tasks or adjust sprint scope/dates"
                    });
                }
            }

            var blockedCount = sprintTasks.Count(t => t.Status == TaskConstants.Statuses.Blocked);
            if (blockedCount > 0)
            {
                risks.Add(new SprintRiskInfo
                {
                    SprintId = sprint.Id,
                    SprintNumber = sprint.Number,
                    RiskType = "BlockedTasks",
                    Severity = blockedCount >= 3 ? "High" : "Medium",
                    Description = $"{blockedCount} project tasks blocked in sprint {sprint.Number}",
                    Recommendation = "Resolve blockers immediately to prevent sprint failure"
                });
            }
        }

        return risks;
    }

    [KernelFunction]
    [Description("Detect task quality and ownership risks for a project")]
    public async Task<List<TaskRiskInfo>> DetectTaskRisks(int projectId)
    {
        var risks = new List<TaskRiskInfo>();

        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId && t.Status != TaskConstants.Statuses.Done)
            .ToListAsync();

        foreach (var task in tasks)
        {
            if (string.IsNullOrWhiteSpace(task.Description) || task.Description.Length < 20)
            {
                risks.Add(new TaskRiskInfo
                {
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    RiskType = "VagueDescription",
                    Severity = "Medium",
                    Description = "Task description is incomplete or too short",
                    Recommendation = "Use AI task improver to add acceptance criteria and more detail"
                });
            }

            if ((task.Priority == TaskConstants.Priorities.High || task.Priority == TaskConstants.Priorities.Critical)
                && task.AssigneeId == null)
            {
                risks.Add(new TaskRiskInfo
                {
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    RiskType = "UnassignedHighPriority",
                    Severity = "High",
                    Description = "High or critical priority task has no assignee",
                    Recommendation = "Assign immediately to prevent delays"
                });
            }
        }

        return risks;
    }
}

public class SprintRiskInfo
{
    public int SprintId { get; set; }
    public int SprintNumber { get; set; }
    public string RiskType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class TaskRiskInfo
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string RiskType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

