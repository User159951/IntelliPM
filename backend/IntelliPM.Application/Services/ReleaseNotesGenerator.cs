using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for generating release notes and changelogs automatically.
/// Analyzes sprints, tasks, and changes to create comprehensive release documentation.
/// </summary>
public class ReleaseNotesGenerator : IReleaseNotesGenerator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReleaseNotesGenerator> _logger;

    public ReleaseNotesGenerator(
        IUnitOfWork unitOfWork,
        ILogger<ReleaseNotesGenerator> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<string> GenerateReleaseNotesAsync(int releaseId, CancellationToken cancellationToken)
    {
        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.Sprints)
                .ThenInclude(s => s.Items)
                .ThenInclude(si => si.UserStory)
                .ThenInclude(us => us.Tasks)
                    .ThenInclude(t => t.Assignee)
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r => r.Id == releaseId, cancellationToken);

        if (release == null)
        {
            throw new NotFoundException($"Release {releaseId} not found");
        }

        var markdown = new StringBuilder();

        // Header
        markdown.AppendLine($"# Release Notes - {release.Name} ({release.Version})");
        markdown.AppendLine();
        markdown.AppendLine($"**Release Date:** {release.PlannedDate:yyyy-MM-dd}");
        markdown.AppendLine($"**Type:** {release.Type}");
        if (release.IsPreRelease)
        {
            markdown.AppendLine("**⚠️ Pre-Release**");
        }
        markdown.AppendLine();

        if (!string.IsNullOrEmpty(release.Description))
        {
            markdown.AppendLine("## Overview");
            markdown.AppendLine(release.Description);
            markdown.AppendLine();
        }

        // Collect all tasks from sprints via SprintItem -> UserStory -> Tasks
        var allTasks = release.Sprints
            .SelectMany(s => s.Items)
            .Select(si => si.UserStory)
            .Where(us => us != null)
            .SelectMany(us => us.Tasks)
            .Where(t => t.Status == TaskConstants.Statuses.Done)
            .ToList();

        // Group tasks by type/category
        var features = allTasks
            .Where(t => IsFeatureTask(t))
            .ToList();

        if (features.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.FeaturesSectionTitle);
            foreach (var task in features)
            {
                markdown.AppendLine($"- {task.Title} (#{task.Id})");
            }
            markdown.AppendLine();
        }

        // Bug fixes
        var bugFixes = allTasks
            .Where(t => IsBugTask(t))
            .ToList();

        if (bugFixes.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.BugFixesSectionTitle);
            foreach (var task in bugFixes)
            {
                markdown.AppendLine($"- {task.Title} (#{task.Id})");
            }
            markdown.AppendLine();
        }

        // Improvements
        var improvements = allTasks
            .Where(t => IsImprovementTask(t))
            .ToList();

        if (improvements.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.ImprovementsSectionTitle);
            foreach (var task in improvements)
            {
                markdown.AppendLine($"- {task.Title} (#{task.Id})");
            }
            markdown.AppendLine();
        }

        // Statistics
        markdown.AppendLine(ReleaseConstants.ReleaseNotes.StatisticsSectionTitle);
        markdown.AppendLine($"- **Total Tasks Completed:** {allTasks.Count}");
        markdown.AppendLine($"- **Sprints Included:** {release.Sprints.Count}");
        markdown.AppendLine($"- **Features:** {features.Count}");
        markdown.AppendLine($"- **Bug Fixes:** {bugFixes.Count}");
        markdown.AppendLine($"- **Improvements:** {improvements.Count}");
        markdown.AppendLine();

        // Sprint breakdown
        if (release.Sprints.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.SprintsSectionTitle);
            foreach (var sprint in release.Sprints.OrderBy(s => s.StartDate))
            {
                var sprintTasks = sprint.Items
                    .Select(si => si.UserStory)
                    .Where(us => us != null)
                    .SelectMany(us => us.Tasks)
                    .Count(t => t.Status == TaskConstants.Statuses.Done);

                var sprintName = $"Sprint {sprint.Number}";
                var startDate = sprint.StartDate?.ToString("yyyy-MM-dd") ?? "TBD";
                var endDate = sprint.EndDate?.ToString("yyyy-MM-dd") ?? "TBD";
                markdown.AppendLine($"- **{sprintName}** ({startDate} - {endDate}): {sprintTasks} tasks completed");
            }
            markdown.AppendLine();
        }

        // Contributors
        var contributors = allTasks
            .Where(t => t.Assignee != null)
            .Select(t => t.Assignee!.Username)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        if (contributors.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.ContributorsSectionTitle);
            markdown.AppendLine(string.Join(", ", contributors.Select(c => $"@{c}")));
            markdown.AppendLine();
        }

        _logger.LogInformation("Generated release notes for Release {ReleaseId} with {TaskCount} tasks", releaseId, allTasks.Count);

        return markdown.ToString();
    }

    public async System.Threading.Tasks.Task<string> GenerateChangeLogAsync(int releaseId, CancellationToken cancellationToken)
    {
        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.Sprints)
                .ThenInclude(s => s.Items)
                .ThenInclude(si => si.UserStory)
                .ThenInclude(us => us.Tasks)
                    .ThenInclude(t => t.Assignee)
            .FirstOrDefaultAsync(r => r.Id == releaseId, cancellationToken);

        if (release == null)
        {
            throw new NotFoundException($"Release {releaseId} not found");
        }

        var markdown = new StringBuilder();

        // Standard changelog format
        markdown.AppendLine($"## [{release.Version}] - {release.PlannedDate:yyyy-MM-dd}");
        markdown.AppendLine();

        var allTasks = release.Sprints
            .SelectMany(s => s.Items)
            .Select(si => si.UserStory)
            .Where(us => us != null)
            .SelectMany(us => us.Tasks)
            .Where(t => t.Status == TaskConstants.Statuses.Done)
            .ToList();

        // Added (features)
        var added = allTasks.Where(t => IsFeatureTask(t)).ToList();
        if (added.Any())
        {
            markdown.AppendLine("### Added");
            foreach (var task in added)
            {
                var assignee = task.Assignee != null ? $" @{task.Assignee.Username}" : "";
                markdown.AppendLine($"- {task.Title} (#{task.Id}){assignee}");
            }
            markdown.AppendLine();
        }

        // Fixed (bugs)
        var fixedTasks = allTasks.Where(t => IsBugTask(t)).ToList();
        if (fixedTasks.Any())
        {
            markdown.AppendLine("### Fixed");
            foreach (var task in fixedTasks)
            {
                var assignee = task.Assignee != null ? $" @{task.Assignee.Username}" : "";
                markdown.AppendLine($"- {task.Title} (#{task.Id}){assignee}");
            }
            markdown.AppendLine();
        }

        // Changed (improvements)
        var changed = allTasks.Where(t => IsImprovementTask(t)).ToList();
        if (changed.Any())
        {
            markdown.AppendLine("### Changed");
            foreach (var task in changed)
            {
                var assignee = task.Assignee != null ? $" @{task.Assignee.Username}" : "";
                markdown.AppendLine($"- {task.Title} (#{task.Id}){assignee}");
            }
            markdown.AppendLine();
        }

        _logger.LogInformation("Generated changelog for Release {ReleaseId} with {TaskCount} tasks", releaseId, allTasks.Count);

        return markdown.ToString();
    }

    public async System.Threading.Tasks.Task<string> GenerateReleaseNotesFromSprintsAsync(int projectId, List<int> sprintIds, CancellationToken cancellationToken)
    {
        var sprints = await _unitOfWork.Repository<Sprint>()
            .Query()
            .Where(s => sprintIds.Contains(s.Id) && s.ProjectId == projectId)
            .Include(s => s.Items)
                .ThenInclude(si => si.UserStory)
                .ThenInclude(us => us.Tasks)
                    .ThenInclude(t => t.Assignee)
            .OrderBy(s => s.StartDate)
            .ToListAsync(cancellationToken);

        if (!sprints.Any())
        {
            return "No sprints found.";
        }

        var markdown = new StringBuilder();

        markdown.AppendLine("# Release Notes - Sprint Selection");
        markdown.AppendLine();

        var allTasks = sprints
            .SelectMany(s => s.Items)
            .Select(si => si.UserStory)
            .Where(us => us != null)
            .SelectMany(us => us.Tasks)
            .Where(t => t.Status == TaskConstants.Statuses.Done)
            .ToList();

        // Features
        var features = allTasks.Where(t => IsFeatureTask(t)).ToList();
        if (features.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.FeaturesSectionTitle);
            foreach (var task in features)
            {
                markdown.AppendLine($"- {task.Title} (#{task.Id})");
            }
            markdown.AppendLine();
        }

        // Bug fixes
        var bugFixes = allTasks.Where(t => IsBugTask(t)).ToList();
        if (bugFixes.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.BugFixesSectionTitle);
            foreach (var task in bugFixes)
            {
                markdown.AppendLine($"- {task.Title} (#{task.Id})");
            }
            markdown.AppendLine();
        }

        // Improvements
        var improvements = allTasks.Where(t => IsImprovementTask(t)).ToList();
        if (improvements.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.ImprovementsSectionTitle);
            foreach (var task in improvements)
            {
                markdown.AppendLine($"- {task.Title} (#{task.Id})");
            }
            markdown.AppendLine();
        }

        // Statistics
        markdown.AppendLine(ReleaseConstants.ReleaseNotes.StatisticsSectionTitle);
        markdown.AppendLine($"- **Total Tasks Completed:** {allTasks.Count}");
        markdown.AppendLine($"- **Sprints Included:** {sprints.Count}");
        markdown.AppendLine($"- **Features:** {features.Count}");
        markdown.AppendLine($"- **Bug Fixes:** {bugFixes.Count}");
        markdown.AppendLine($"- **Improvements:** {improvements.Count}");
        markdown.AppendLine();

        // Sprint breakdown
        markdown.AppendLine(ReleaseConstants.ReleaseNotes.SprintsSectionTitle);
        foreach (var sprint in sprints)
        {
            var sprintTasks = sprint.Items
                .Select(si => si.UserStory)
                .Where(us => us != null)
                .SelectMany(us => us.Tasks)
                .Count(t => t.Status == "DONE" || t.Status == "Done");

            var sprintName = $"Sprint {sprint.Number}";
            var startDate = sprint.StartDate?.ToString("yyyy-MM-dd") ?? "TBD";
            var endDate = sprint.EndDate?.ToString("yyyy-MM-dd") ?? "TBD";
            markdown.AppendLine($"- **{sprintName}** ({startDate} - {endDate}): {sprintTasks} tasks completed");
        }
        markdown.AppendLine();

        // Contributors
        var contributors = allTasks
            .Where(t => t.Assignee != null)
            .Select(t => t.Assignee!.Username)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        if (contributors.Any())
        {
            markdown.AppendLine(ReleaseConstants.ReleaseNotes.ContributorsSectionTitle);
            markdown.AppendLine(string.Join(", ", contributors.Select(c => $"@{c}")));
            markdown.AppendLine();
        }

        _logger.LogInformation("Generated release notes from {SprintCount} sprints with {TaskCount} tasks", sprints.Count, allTasks.Count);

        return markdown.ToString();
    }

    /// <summary>
    /// Determines if a task is a feature based on heuristics.
    /// Checks task title for feature-related keywords and considers task characteristics.
    /// </summary>
    private bool IsFeatureTask(IntelliPM.Domain.Entities.Task task)
    {
        var titleLower = task.Title.ToLower();
        
        // Exclude bug fixes and improvements
        if (IsBugTask(task) || IsImprovementTask(task))
        {
            return false;
        }

        // Feature keywords
        var featureKeywords = new[] { "feature", "add", "new", "implement", "create", "introduce", "support" };
        if (featureKeywords.Any(keyword => titleLower.Contains(keyword)))
        {
            return true;
        }

        // Default: if not bug or improvement, consider it a feature
        return true;
    }

    /// <summary>
    /// Determines if a task is a bug fix based on heuristics.
    /// Checks task title for bug-related keywords.
    /// </summary>
    private bool IsBugTask(IntelliPM.Domain.Entities.Task task)
    {
        var titleLower = task.Title.ToLower();
        var bugKeywords = new[] { "bug", "fix", "error", "issue", "defect", "crash", "broken", "resolve", "patch" };
        return bugKeywords.Any(keyword => titleLower.Contains(keyword));
    }

    /// <summary>
    /// Determines if a task is an improvement based on heuristics.
    /// Checks task title for improvement-related keywords.
    /// </summary>
    private bool IsImprovementTask(IntelliPM.Domain.Entities.Task task)
    {
        var titleLower = task.Title.ToLower();
        var improvementKeywords = new[] { "improve", "enhance", "optimize", "refactor", "update", "upgrade", "performance", "better", "faster" };
        return improvementKeywords.Any(keyword => titleLower.Contains(keyword));
    }
}
