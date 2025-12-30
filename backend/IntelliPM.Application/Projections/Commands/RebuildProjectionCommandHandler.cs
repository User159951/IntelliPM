using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Projections.Commands;

/// <summary>
/// Handler for rebuilding read model projections from source data.
/// Admin-only operation that reconstructs projections from current write model state.
/// </summary>
public class RebuildProjectionCommandHandler : IRequestHandler<RebuildProjectionCommand, RebuildProjectionResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RebuildProjectionCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public RebuildProjectionCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RebuildProjectionCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<RebuildProjectionResponse> Handle(RebuildProjectionCommand request, CancellationToken ct)
    {
        // Check if user is admin
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can rebuild projections");
        }

        var startTime = DateTime.UtcNow;
        var details = new List<string>();
        var projectionsRebuilt = 0;

        try
        {
            _logger.LogInformation(
                "Starting projection rebuild: Type={ProjectionType}, ProjectId={ProjectId}, OrganizationId={OrganizationId}, ForceRebuild={ForceRebuild}",
                request.ProjectionType,
                request.ProjectId,
                request.OrganizationId,
                request.ForceRebuild);

            // Build project filter
            var projectIds = await GetProjectIdsAsync(request.ProjectId, request.OrganizationId, ct);

            if (!projectIds.Any())
            {
                details.Add("No projects found matching the specified criteria");
                _logger.LogWarning("No projects found for projection rebuild");
            }
            else
            {
                details.Add($"Found {projectIds.Count} project(s) to process");

                // Rebuild based on type
                switch (request.ProjectionType.ToLower())
                {
                    case "all":
                        projectionsRebuilt += await RebuildTaskBoardProjectionsAsync(projectIds, request.ForceRebuild, details, ct);
                        projectionsRebuilt += await RebuildSprintSummaryProjectionsAsync(projectIds, request.ForceRebuild, details, ct);
                        projectionsRebuilt += await RebuildProjectOverviewProjectionsAsync(projectIds, request.ForceRebuild, details, ct);
                        break;

                    case "taskboard":
                        projectionsRebuilt += await RebuildTaskBoardProjectionsAsync(projectIds, request.ForceRebuild, details, ct);
                        break;

                    case "sprintsummary":
                        projectionsRebuilt += await RebuildSprintSummaryProjectionsAsync(projectIds, request.ForceRebuild, details, ct);
                        break;

                    case "projectoverview":
                        projectionsRebuilt += await RebuildProjectOverviewProjectionsAsync(projectIds, request.ForceRebuild, details, ct);
                        break;

                    default:
                        throw new ValidationException($"Unknown projection type: {request.ProjectionType}");
                }

                await _unitOfWork.SaveChangesAsync(ct);
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Projection rebuild completed: {Count} projections rebuilt in {Duration}ms",
                projectionsRebuilt,
                duration.TotalMilliseconds);

            return new RebuildProjectionResponse(
                projectionsRebuilt,
                details,
                duration,
                true,
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding projections");
            return new RebuildProjectionResponse(
                projectionsRebuilt,
                details,
                DateTime.UtcNow - startTime,
                false,
                ex.Message
            );
        }
    }

    private async System.Threading.Tasks.Task<List<int>> GetProjectIdsAsync(int? projectId, int? organizationId, CancellationToken ct)
    {
        var query = _unitOfWork.Repository<Project>().Query();

        if (projectId.HasValue)
        {
            query = query.Where(p => p.Id == projectId.Value);
        }
        else if (organizationId.HasValue)
        {
            query = query.Where(p => p.OrganizationId == organizationId.Value);
        }

        return await query.Select(p => p.Id).ToListAsync(ct);
    }

    private async System.Threading.Tasks.Task<int> RebuildTaskBoardProjectionsAsync(List<int> projectIds, bool forceRebuild, List<string> details, CancellationToken ct)
    {
        int count = 0;

        foreach (var projectId in projectIds)
        {
            try
            {
                // Delete existing if force rebuild
                if (forceRebuild)
                {
                    var existing = await _unitOfWork.Repository<TaskBoardReadModel>()
                        .Query()
                        .FirstOrDefaultAsync(r => r.ProjectId == projectId, ct);

                    if (existing != null)
                    {
                        _unitOfWork.Repository<TaskBoardReadModel>().Delete(existing);
                        await _unitOfWork.SaveChangesAsync(ct);
                        details.Add($"Deleted existing TaskBoardReadModel for project {projectId}");
                    }
                }

                // Get or create read model
                var readModel = await _unitOfWork.Repository<TaskBoardReadModel>()
                    .Query()
                    .FirstOrDefaultAsync(r => r.ProjectId == projectId, ct);

                var project = await _unitOfWork.Repository<Project>()
                    .GetByIdAsync(projectId, ct);

                if (project == null)
                {
                    details.Add($"Project {projectId} not found, skipping");
                    continue;
                }

                if (readModel == null)
                {
                    readModel = new TaskBoardReadModel
                    {
                        ProjectId = projectId,
                        OrganizationId = project.OrganizationId,
                        LastUpdated = DateTimeOffset.UtcNow,
                        Version = 1
                    };
                    await _unitOfWork.Repository<TaskBoardReadModel>().AddAsync(readModel, ct);
                }

                // Fetch all tasks for project
                var tasks = await _unitOfWork.Repository<ProjectTask>()
                    .Query()
                    .Include(t => t.Assignee)
                    .Where(t => t.ProjectId == projectId)
                    .ToListAsync(ct);

                // Group tasks by status
                var todoTasks = new List<TaskSummaryDto>();
                var inProgressTasks = new List<TaskSummaryDto>();
                var doneTasks = new List<TaskSummaryDto>();

                foreach (var task in tasks)
                {
                    var taskSummary = new TaskSummaryDto
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Priority = task.Priority,
                        StoryPoints = task.StoryPoints?.Value,
                        AssigneeId = task.AssigneeId,
                        AssigneeName = task.Assignee != null
                            ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim()
                            : null,
                        AssigneeAvatar = null,
                        DueDate = null, // DueDate not in ProjectTask entity
                        DisplayOrder = 0
                    };

                    switch (task.Status)
                    {
                        case "Todo":
                        case "Review":
                        case "Blocked":
                            todoTasks.Add(taskSummary);
                            break;
                        case "InProgress":
                            inProgressTasks.Add(taskSummary);
                            break;
                        case "Done":
                            doneTasks.Add(taskSummary);
                            break;
                    }
                }

                // Update read model
                readModel.SetTodoTasks(todoTasks);
                readModel.SetInProgressTasks(inProgressTasks);
                readModel.SetDoneTasks(doneTasks);
                readModel.UpdateCounts();
                readModel.LastUpdated = DateTimeOffset.UtcNow;
                readModel.Version++;

                count++;
                details.Add($"Rebuilt TaskBoard for project {projectId}: {tasks.Count} tasks ({todoTasks.Count} Todo, {inProgressTasks.Count} InProgress, {doneTasks.Count} Done)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding TaskBoard for project {ProjectId}", projectId);
                details.Add($"Failed to rebuild TaskBoard for project {projectId}: {ex.Message}");
            }
        }

        return count;
    }

    private async System.Threading.Tasks.Task<int> RebuildSprintSummaryProjectionsAsync(List<int> projectIds, bool forceRebuild, List<string> details, CancellationToken ct)
    {
        int count = 0;

        // Get all sprints for the specified projects
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var sprints = await sprintRepo.Query()
            .Where(s => projectIds.Contains(s.ProjectId))
            .ToListAsync(ct);

        foreach (var sprint in sprints)
        {
            try
            {
                // Delete existing if force rebuild
                if (forceRebuild)
                {
                    var existing = await _unitOfWork.Repository<SprintSummaryReadModel>()
                        .Query()
                        .FirstOrDefaultAsync(r => r.SprintId == sprint.Id, ct);

                    if (existing != null)
                    {
                        _unitOfWork.Repository<SprintSummaryReadModel>().Delete(existing);
                        await _unitOfWork.SaveChangesAsync(ct);
                        details.Add($"Deleted existing SprintSummaryReadModel for sprint {sprint.Id}");
                    }
                }

                // Get or create read model
                var readModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
                    .Query()
                    .FirstOrDefaultAsync(r => r.SprintId == sprint.Id, ct);

                if (readModel == null)
                {
                    readModel = new SprintSummaryReadModel
                    {
                        SprintId = sprint.Id,
                        ProjectId = sprint.ProjectId,
                        OrganizationId = sprint.OrganizationId,
                        SprintName = $"Sprint {sprint.Number}",
                        Status = sprint.Status,
                        StartDate = sprint.StartDate ?? DateTimeOffset.UtcNow,
                        EndDate = sprint.EndDate ?? DateTimeOffset.UtcNow.AddDays(14),
                        LastUpdated = DateTimeOffset.UtcNow,
                        Version = 1
                    };
                    await _unitOfWork.Repository<SprintSummaryReadModel>().AddAsync(readModel, ct);
                }
                else
                {
                    // Update basic info
                    readModel.SprintName = $"Sprint {sprint.Number}";
                    readModel.Status = sprint.Status;
                    readModel.StartDate = sprint.StartDate ?? DateTimeOffset.UtcNow;
                    readModel.EndDate = sprint.EndDate ?? DateTimeOffset.UtcNow.AddDays(14);
                }

                // Get all tasks for this sprint
                var taskRepo = _unitOfWork.Repository<ProjectTask>();
                var sprintTasks = await taskRepo.Query()
                    .Where(t => t.SprintId == sprint.Id)
                    .ToListAsync(ct);

                // Calculate task counts by status
                readModel.TotalTasks = sprintTasks.Count;
                readModel.CompletedTasks = sprintTasks.Count(t => t.Status == "Done");
                readModel.InProgressTasks = sprintTasks.Count(t => t.Status == "InProgress");
                readModel.TodoTasks = sprintTasks.Count(t => t.Status == "Todo" || t.Status == "Review" || t.Status == "Blocked");

                // Calculate story points
                readModel.TotalStoryPoints = sprintTasks.Sum(t => t.StoryPoints?.Value ?? 0);
                readModel.CompletedStoryPoints = sprintTasks
                    .Where(t => t.Status == "Done")
                    .Sum(t => t.StoryPoints?.Value ?? 0);
                readModel.InProgressStoryPoints = sprintTasks
                    .Where(t => t.Status == "InProgress")
                    .Sum(t => t.StoryPoints?.Value ?? 0);

                // Recalculate metrics
                readModel.RecalculateMetrics();

                // Calculate burndown data
                await CalculateBurndownDataAsync(readModel, sprintTasks, ct);

                // Calculate average velocity from previous sprints
                await CalculateAverageVelocityAsync(readModel, ct);

                readModel.Version++;
                readModel.LastUpdated = DateTimeOffset.UtcNow;

                count++;
                details.Add($"Rebuilt SprintSummary for sprint {sprint.Id} (Sprint {sprint.Number}): {sprintTasks.Count} tasks, {readModel.CompletedStoryPoints}/{readModel.TotalStoryPoints} story points completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding SprintSummary for sprint {SprintId}", sprint.Id);
                details.Add($"Failed to rebuild SprintSummary for sprint {sprint.Id}: {ex.Message}");
            }
        }

        return count;
    }

    private async System.Threading.Tasks.Task<int> RebuildProjectOverviewProjectionsAsync(List<int> projectIds, bool forceRebuild, List<string> details, CancellationToken ct)
    {
        int count = 0;

        foreach (var projectId in projectIds)
        {
            try
            {
                // Delete existing if force rebuild
                if (forceRebuild)
                {
                    var existing = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                        .Query()
                        .FirstOrDefaultAsync(r => r.ProjectId == projectId, ct);

                    if (existing != null)
                    {
                        _unitOfWork.Repository<ProjectOverviewReadModel>().Delete(existing);
                        await _unitOfWork.SaveChangesAsync(ct);
                        details.Add($"Deleted existing ProjectOverviewReadModel for project {projectId}");
                    }
                }

                // Get or create read model
                var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                    .Query()
                    .FirstOrDefaultAsync(r => r.ProjectId == projectId, ct);

                var project = await _unitOfWork.Repository<Project>()
                    .GetByIdAsync(projectId, ct);

                if (project == null)
                {
                    details.Add($"Project {projectId} not found, skipping");
                    continue;
                }

                if (readModel == null)
                {
                    var owner = await _unitOfWork.Repository<User>()
                        .GetByIdAsync(project.OwnerId, ct);

                    readModel = new ProjectOverviewReadModel
                    {
                        ProjectId = projectId,
                        OrganizationId = project.OrganizationId,
                        ProjectName = project.Name,
                        ProjectType = project.Type,
                        Status = project.Status,
                        OwnerId = project.OwnerId,
                        OwnerName = owner != null
                            ? $"{owner.FirstName} {owner.LastName}".Trim()
                            : "Unknown",
                        LastUpdated = DateTimeOffset.UtcNow,
                        Version = 1
                    };
                    await _unitOfWork.Repository<ProjectOverviewReadModel>().AddAsync(readModel, ct);
                }
                else
                {
                    // Update basic info
                    readModel.ProjectName = project.Name;
                    readModel.ProjectType = project.Type;
                    readModel.Status = project.Status;

                    var owner = await _unitOfWork.Repository<User>()
                        .GetByIdAsync(project.OwnerId, ct);
                    readModel.OwnerName = owner != null
                        ? $"{owner.FirstName} {owner.LastName}".Trim()
                        : "Unknown";
                }

                // Calculate team statistics
                var memberRepo = _unitOfWork.Repository<ProjectMember>();
                var members = await memberRepo.Query()
                    .Include(m => m.User)
                    .Where(m => m.ProjectId == projectId)
                    .ToListAsync(ct);

                readModel.TotalMembers = members.Count;
                readModel.ActiveMembers = members.Count;

                // Build team member summaries
                var taskRepo = _unitOfWork.Repository<ProjectTask>();
                var memberSummaries = new List<MemberSummaryDto>();

                foreach (var member in members)
                {
                    var tasksAssigned = await taskRepo.Query()
                        .CountAsync(t => t.ProjectId == projectId && t.AssigneeId == member.UserId, ct);

                    var tasksCompleted = await taskRepo.Query()
                        .CountAsync(t => t.ProjectId == projectId &&
                                       t.AssigneeId == member.UserId &&
                                       t.Status == "Done", ct);

                    memberSummaries.Add(new MemberSummaryDto
                    {
                        UserId = member.UserId,
                        Username = member.User?.Username ?? "Unknown",
                        Role = member.Role.ToString(),
                        TasksAssigned = tasksAssigned,
                        TasksCompleted = tasksCompleted
                    });
                }

                readModel.SetTeamMembers(memberSummaries);

                // Calculate sprint statistics
                var sprintRepo = _unitOfWork.Repository<Sprint>();
                var sprints = await sprintRepo.Query()
                    .Where(s => s.ProjectId == projectId)
                    .ToListAsync(ct);

                readModel.TotalSprints = sprints.Count;
                readModel.ActiveSprintsCount = sprints.Count(s => s.Status == "Active");
                readModel.CompletedSprintsCount = sprints.Count(s => s.Status == "Completed");

                var currentSprint = sprints.FirstOrDefault(s => s.Status == "Active");
                readModel.CurrentSprintId = currentSprint?.Id;
                readModel.CurrentSprintName = currentSprint != null ? $"Sprint {currentSprint.Number}" : null;

                // Calculate task statistics
                var tasks = await taskRepo.Query()
                    .Where(t => t.ProjectId == projectId)
                    .ToListAsync(ct);

                readModel.TotalTasks = tasks.Count;
                readModel.CompletedTasks = tasks.Count(t => t.Status == "Done");
                readModel.InProgressTasks = tasks.Count(t => t.Status == "InProgress");
                readModel.TodoTasks = tasks.Count(t => t.Status == "Todo" || t.Status == "Review" || t.Status == "Blocked");
                readModel.BlockedTasks = tasks.Count(t => t.Status == "Blocked");
                readModel.OverdueTasks = 0; // Would need DueDate to calculate

                // Calculate story points
                readModel.TotalStoryPoints = tasks.Sum(t => t.StoryPoints?.Value ?? 0);
                readModel.CompletedStoryPoints = tasks
                    .Where(t => t.Status == "Done")
                    .Sum(t => t.StoryPoints?.Value ?? 0);

                // Calculate defect statistics
                var defectRepo = _unitOfWork.Repository<Defect>();
                var defects = await defectRepo.Query()
                    .Where(d => d.ProjectId == projectId)
                    .ToListAsync(ct);

                readModel.TotalDefects = defects.Count;
                readModel.OpenDefects = defects.Count(d => d.Status != "Resolved" && d.Status != "Closed");
                readModel.CriticalDefects = defects.Count(d => d.Severity == "Critical");

                // Calculate velocity metrics
                await CalculateVelocityMetricsAsync(readModel, sprints, ct);

                // Calculate activity metrics
                await CalculateActivityMetricsAsync(readModel, projectId, ct);

                // Recalculate health and progress
                readModel.CalculateHealth();

                // Calculate sprint progress
                if (currentSprint != null)
                {
                    var sprintTasks = tasks.Where(t => t.SprintId == currentSprint.Id).ToList();
                    var sprintTotalTasks = sprintTasks.Count;
                    var sprintCompletedTasks = sprintTasks.Count(t => t.Status == "Done");
                    readModel.SprintProgress = sprintTotalTasks > 0
                        ? Math.Round((decimal)sprintCompletedTasks / sprintTotalTasks * 100, 2)
                        : 0;
                }
                else
                {
                    readModel.SprintProgress = 0;
                }

                readModel.Version++;
                readModel.LastUpdated = DateTimeOffset.UtcNow;

                count++;
                details.Add($"Rebuilt ProjectOverview for project {projectId} ({project.Name}): {readModel.TotalMembers} members, {readModel.TotalTasks} tasks, {readModel.TotalSprints} sprints, Health={readModel.ProjectHealth:F1}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding ProjectOverview for project {ProjectId}", projectId);
                details.Add($"Failed to rebuild ProjectOverview for project {projectId}: {ex.Message}");
            }
        }

        return count;
    }

    private async System.Threading.Tasks.Task CalculateBurndownDataAsync(SprintSummaryReadModel readModel, List<ProjectTask> sprintTasks, CancellationToken ct)
    {
        var startDate = readModel.StartDate.Date;
        var endDate = readModel.EndDate.Date;
        var totalDays = (endDate - startDate).Days;

        if (totalDays <= 0)
        {
            readModel.SetBurndownData(new List<BurndownPointDto>());
            return;
        }

        var totalStoryPoints = readModel.TotalStoryPoints;
        var idealBurnRate = (decimal)totalStoryPoints / totalDays;

        var burndownData = new List<BurndownPointDto>();
        var today = DateTime.UtcNow.Date;

        // Generate burndown points for each day
        for (int day = 0; day <= totalDays && startDate.AddDays(day) <= today; day++)
        {
            var currentDate = startDate.AddDays(day);

            // Calculate ideal remaining points
            var idealRemaining = Math.Max(0, totalStoryPoints - (idealBurnRate * day));

            // Calculate actual remaining points (for simplicity, use current state)
            var actualRemaining = readModel.TotalStoryPoints - readModel.CompletedStoryPoints;

            burndownData.Add(new BurndownPointDto
            {
                Date = currentDate,
                RemainingStoryPoints = (int)actualRemaining,
                IdealRemainingPoints = (int)idealRemaining
            });
        }

        readModel.SetBurndownData(burndownData);
    }

    private async System.Threading.Tasks.Task CalculateAverageVelocityAsync(SprintSummaryReadModel readModel, CancellationToken ct)
    {
        // Get all completed sprints for this project
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var completedSprints = await sprintRepo.Query()
            .Where(s => s.ProjectId == readModel.ProjectId &&
                       s.Status == "Completed" &&
                       s.Id != readModel.SprintId)
            .OrderByDescending(s => s.EndDate ?? s.CreatedAt)
            .Take(5) // Last 5 sprints
            .ToListAsync(ct);

        if (completedSprints.Any())
        {
            var sprintIds = completedSprints.Select(s => s.Id).ToList();
            var taskRepo = _unitOfWork.Repository<ProjectTask>();

            var completedStoryPoints = await taskRepo.Query()
                .Where(t => sprintIds.Contains(t.SprintId ?? 0) && t.Status == "Done")
                .SumAsync(t => t.StoryPoints != null ? t.StoryPoints.Value : 0, ct);

            readModel.AverageVelocity = completedSprints.Count > 0
                ? Math.Round((decimal)completedStoryPoints / completedSprints.Count, 2)
                : 0;
        }
        else
        {
            readModel.AverageVelocity = 0;
        }
    }

    private async System.Threading.Tasks.Task CalculateVelocityMetricsAsync(ProjectOverviewReadModel readModel, List<Sprint> sprints, CancellationToken ct)
    {
        var completedSprints = sprints.Where(s => s.Status == "Completed").ToList();

        if (completedSprints.Any())
        {
            var sprintIds = completedSprints.Select(s => s.Id).ToList();
            var taskRepo = _unitOfWork.Repository<ProjectTask>();

            var completedStoryPoints = await taskRepo.Query()
                .Where(t => sprintIds.Contains(t.SprintId ?? 0) && t.Status == "Done")
                .SumAsync(t => t.StoryPoints != null ? t.StoryPoints.Value : 0, ct);

            readModel.AverageVelocity = completedSprints.Count > 0
                ? Math.Round((decimal)completedStoryPoints / completedSprints.Count, 2)
                : 0;

            // Last sprint velocity
            var lastSprint = completedSprints
                .OrderByDescending(s => s.EndDate ?? s.CreatedAt)
                .FirstOrDefault();

            if (lastSprint != null)
            {
                var lastSprintTasks = await taskRepo.Query()
                    .Where(t => t.SprintId == lastSprint.Id && t.Status == "Done")
                    .ToListAsync(ct);

                readModel.LastSprintVelocity = lastSprintTasks.Sum(t => t.StoryPoints?.Value ?? 0);
            }

            // Velocity trend
            var velocityTrend = new List<VelocityTrendDto>();
            foreach (var sprint in completedSprints.OrderBy(s => s.EndDate ?? s.CreatedAt))
            {
                var sprintTasks = await taskRepo.Query()
                    .Where(t => t.SprintId == sprint.Id && t.Status == "Done")
                    .ToListAsync(ct);

                velocityTrend.Add(new VelocityTrendDto
                {
                    SprintName = $"Sprint {sprint.Number}",
                    Velocity = sprintTasks.Sum(t => t.StoryPoints?.Value ?? 0),
                    Date = sprint.EndDate?.DateTime ?? sprint.CreatedAt.DateTime
                });
            }

            readModel.SetVelocityTrend(velocityTrend);
        }
        else
        {
            readModel.AverageVelocity = 0;
            readModel.LastSprintVelocity = 0;
            readModel.SetVelocityTrend(new List<VelocityTrendDto>());
        }
    }

    private async System.Threading.Tasks.Task CalculateActivityMetricsAsync(ProjectOverviewReadModel readModel, int projectId, CancellationToken ct)
    {
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        var activities = await activityRepo.Query()
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        readModel.LastActivityAt = activities.FirstOrDefault()?.CreatedAt ?? DateTimeOffset.UtcNow;

        var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

        readModel.ActivitiesLast7Days = activities.Count(a => a.CreatedAt >= sevenDaysAgo);
        readModel.ActivitiesLast30Days = activities.Count(a => a.CreatedAt >= thirtyDaysAgo);
    }
}

