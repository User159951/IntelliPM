using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskEntity = IntelliPM.Domain.Entities.Task;

namespace IntelliPM.Application.Projections;

/// <summary>
/// Projection handler that updates ProjectOverviewReadModel when project-related domain events occur.
/// Implements eventual consistency pattern - handlers are idempotent and safe to retry.
/// </summary>
public class ProjectProjectionHandler :
    INotificationHandler<ProjectCreatedEvent>,
    INotificationHandler<ProjectUpdatedEvent>,
    INotificationHandler<MemberAddedToProjectEvent>,
    INotificationHandler<MemberRemovedFromProjectEvent>,
    INotificationHandler<SprintCreatedEvent>,
    INotificationHandler<SprintStartedEvent>,
    INotificationHandler<SprintCompletedEvent>,
    INotificationHandler<TaskCreatedEvent>,
    INotificationHandler<TaskUpdatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProjectProjectionHandler> _logger;

    public ProjectProjectionHandler(
        IUnitOfWork unitOfWork,
        ILogger<ProjectProjectionHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task Handle(ProjectCreatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for project created: {ProjectId}", notification.ProjectId);

            var readModel = await GetOrCreateProjectOverviewReadModelAsync(notification.ProjectId, notification.OrganizationId, ct);

            // Update basic project info
            readModel.ProjectName = notification.ProjectName;
            readModel.ProjectType = notification.ProjectType;
            readModel.Status = notification.Status;
            readModel.OwnerId = notification.OwnerId;

            // Get owner name
            var owner = await _unitOfWork.Repository<User>()
                .GetByIdAsync(notification.OwnerId, ct);
            readModel.OwnerName = owner != null 
                ? $"{owner.FirstName} {owner.LastName}".Trim() 
                : "Unknown";

            readModel.Version++;
            readModel.LastUpdated = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for project: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for project created: {ProjectId}", notification.ProjectId);
        }
    }

    public async System.Threading.Tasks.Task Handle(ProjectUpdatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for project updated: {ProjectId}", notification.ProjectId);

            var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("ProjectOverviewReadModel for project {ProjectId} not found when processing ProjectUpdatedEvent", notification.ProjectId);
                return;
            }

            // Recalculate all metrics
            await RecalculateProjectMetricsAsync(readModel, ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for project: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for project updated: {ProjectId}", notification.ProjectId);
        }
    }

    public async System.Threading.Tasks.Task Handle(MemberAddedToProjectEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for member added to project: {ProjectId}, User: {UserId}", notification.ProjectId, notification.UserId);

            var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("ProjectOverviewReadModel for project {ProjectId} not found when processing MemberAddedToProjectEvent", notification.ProjectId);
                return;
            }

            // Recalculate team statistics
            await RecalculateProjectMetricsAsync(readModel, ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for member added: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for member added: {ProjectId}", notification.ProjectId);
        }
    }

    public async System.Threading.Tasks.Task Handle(MemberRemovedFromProjectEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for member removed from project: {ProjectId}, User: {UserId}", notification.ProjectId, notification.UserId);

            var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("ProjectOverviewReadModel for project {ProjectId} not found when processing MemberRemovedFromProjectEvent", notification.ProjectId);
                return;
            }

            // Recalculate team statistics
            await RecalculateProjectMetricsAsync(readModel, ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for member removed: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for member removed: {ProjectId}", notification.ProjectId);
        }
    }

    public async System.Threading.Tasks.Task Handle(SprintCreatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for sprint created in project: {ProjectId}", notification.ProjectId);

            var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("ProjectOverviewReadModel for project {ProjectId} not found when processing SprintCreatedEvent", notification.ProjectId);
                return;
            }

            await RecalculateProjectMetricsAsync(readModel, ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for sprint created: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for sprint created: {ProjectId}", notification.ProjectId);
        }
    }

    public async System.Threading.Tasks.Task Handle(SprintStartedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for sprint started in project: {ProjectId}", notification.ProjectId);

            var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("ProjectOverviewReadModel for project {ProjectId} not found when processing SprintStartedEvent", notification.ProjectId);
                return;
            }

            await RecalculateProjectMetricsAsync(readModel, ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for sprint started: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for sprint started: {ProjectId}", notification.ProjectId);
        }
    }

    public async System.Threading.Tasks.Task Handle(SprintCompletedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for sprint completed in project: {ProjectId}", notification.ProjectId);

            var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("ProjectOverviewReadModel for project {ProjectId} not found when processing SprintCompletedEvent", notification.ProjectId);
                return;
            }

            await RecalculateProjectMetricsAsync(readModel, ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for sprint completed: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for sprint completed: {ProjectId}", notification.ProjectId);
        }
    }

    public async System.Threading.Tasks.Task Handle(TaskCreatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for task created in project: {ProjectId}", notification.ProjectId);

            var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("ProjectOverviewReadModel for project {ProjectId} not found when processing TaskCreatedEvent", notification.ProjectId);
                return;
            }

            await RecalculateProjectMetricsAsync(readModel, ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for task created: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for task created: {ProjectId}", notification.ProjectId);
        }
    }

    public async System.Threading.Tasks.Task Handle(TaskUpdatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating ProjectOverviewReadModel for task updated in project: {ProjectId}", notification.ProjectId);

            var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("ProjectOverviewReadModel for project {ProjectId} not found when processing TaskUpdatedEvent", notification.ProjectId);
                return;
            }

            await RecalculateProjectMetricsAsync(readModel, ct);

            _logger.LogInformation("ProjectOverviewReadModel updated successfully for task updated: {ProjectId}", notification.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectOverviewReadModel for task updated: {ProjectId}", notification.ProjectId);
        }
    }

    private async System.Threading.Tasks.Task<ProjectOverviewReadModel> GetOrCreateProjectOverviewReadModelAsync(int projectId, int organizationId, CancellationToken ct)
    {
        var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
            .Query()
            .FirstOrDefaultAsync(r => r.ProjectId == projectId, ct);

        if (readModel == null)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetByIdAsync(projectId, ct);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found");
            }

            var owner = await _unitOfWork.Repository<User>()
                .GetByIdAsync(project.OwnerId, ct);

            readModel = new ProjectOverviewReadModel
            {
                ProjectId = projectId,
                OrganizationId = organizationId,
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

        return readModel;
    }

    private async System.Threading.Tasks.Task RecalculateProjectMetricsAsync(ProjectOverviewReadModel readModel, CancellationToken ct)
    {
        var projectId = readModel.ProjectId;

        // Get project to ensure it exists
        var project = await _unitOfWork.Repository<Project>()
            .GetByIdAsync(projectId, ct);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found when recalculating metrics", projectId);
            return;
        }

        // Update basic info
        readModel.ProjectName = project.Name;
        readModel.ProjectType = project.Type;
        readModel.Status = project.Status;

        // Calculate team statistics
        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        var members = await memberRepo.Query()
            .Include(m => m.User)
            .Where(m => m.ProjectId == projectId)
            .ToListAsync(ct);

        readModel.TotalMembers = members.Count;
        readModel.ActiveMembers = members.Count; // All members are considered active

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

        // Calculate defect statistics (if Defect entity exists)
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

        await _unitOfWork.SaveChangesAsync(ct);
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

