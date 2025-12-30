using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskEntity = IntelliPM.Domain.Entities.Task;

namespace IntelliPM.Application.Projections;

/// <summary>
/// Projection handler that updates SprintSummaryReadModel when sprint-related domain events occur.
/// Implements eventual consistency pattern - handlers are idempotent and safe to retry.
/// </summary>
public class SprintProjectionHandler :
    INotificationHandler<SprintCreatedEvent>,
    INotificationHandler<SprintUpdatedEvent>,
    INotificationHandler<SprintStartedEvent>,
    INotificationHandler<SprintCompletedEvent>,
    INotificationHandler<TaskCreatedEvent>,
    INotificationHandler<TaskUpdatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SprintProjectionHandler> _logger;

    public SprintProjectionHandler(
        IUnitOfWork unitOfWork,
        ILogger<SprintProjectionHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task Handle(SprintCreatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating SprintSummaryReadModel for sprint created: {SprintId}", notification.SprintId);

            var readModel = await GetOrCreateSprintSummaryReadModelAsync(notification.SprintId, notification.ProjectId, notification.OrganizationId, ct);

            // Update basic sprint info
            readModel.SprintName = $"Sprint {notification.Number}";
            readModel.Status = notification.Status;
            readModel.StartDate = notification.StartDate ?? DateTimeOffset.UtcNow;
            readModel.EndDate = notification.EndDate ?? DateTimeOffset.UtcNow.AddDays(14);
            readModel.PlannedCapacity = null; // Not available in Sprint entity currently

            readModel.Version++;
            readModel.LastUpdated = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("SprintSummaryReadModel updated successfully for sprint: {SprintId}", notification.SprintId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SprintSummaryReadModel for sprint created: {SprintId}", notification.SprintId);
        }
    }

    public async System.Threading.Tasks.Task Handle(SprintUpdatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating SprintSummaryReadModel for sprint updated: {SprintId}", notification.SprintId);

            var readModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.SprintId == notification.SprintId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("SprintSummaryReadModel for sprint {SprintId} not found when processing SprintUpdatedEvent", notification.SprintId);
                return;
            }

            // Recalculate metrics from current sprint state
            await RecalculateSprintMetricsAsync(readModel, ct);

            _logger.LogInformation("SprintSummaryReadModel updated successfully for sprint: {SprintId}", notification.SprintId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SprintSummaryReadModel for sprint updated: {SprintId}", notification.SprintId);
        }
    }

    public async System.Threading.Tasks.Task Handle(SprintStartedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating SprintSummaryReadModel for sprint started: {SprintId}", notification.SprintId);

            var readModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.SprintId == notification.SprintId, ct);

            if (readModel == null)
            {
                readModel = await GetOrCreateSprintSummaryReadModelAsync(notification.SprintId, notification.ProjectId, notification.OrganizationId, ct);
            }

            readModel.Status = "Active";
            readModel.StartDate = notification.StartDate;
            readModel.EndDate = notification.EndDate;

            // Recalculate metrics
            await RecalculateSprintMetricsAsync(readModel, ct);

            _logger.LogInformation("SprintSummaryReadModel updated successfully for sprint started: {SprintId}", notification.SprintId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SprintSummaryReadModel for sprint started: {SprintId}", notification.SprintId);
        }
    }

    public async System.Threading.Tasks.Task Handle(SprintCompletedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating SprintSummaryReadModel for sprint completed: {SprintId}", notification.SprintId);

            var readModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.SprintId == notification.SprintId, ct);

            if (readModel == null)
            {
                readModel = await GetOrCreateSprintSummaryReadModelAsync(notification.SprintId, notification.ProjectId, notification.OrganizationId, ct);
            }

            readModel.Status = "Completed";

            // Recalculate final metrics
            await RecalculateSprintMetricsAsync(readModel, ct);

            _logger.LogInformation("SprintSummaryReadModel updated successfully for sprint completed: {SprintId}", notification.SprintId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SprintSummaryReadModel for sprint completed: {SprintId}", notification.SprintId);
        }
    }

    public async System.Threading.Tasks.Task Handle(TaskCreatedEvent notification, CancellationToken ct)
    {
        // Only update if task belongs to a sprint
        if (!notification.SprintId.HasValue)
            return;

        try
        {
            _logger.LogInformation("Updating SprintSummaryReadModel for task created in sprint: {SprintId}", notification.SprintId);

            var readModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.SprintId == notification.SprintId.Value, ct);

            if (readModel == null)
            {
                _logger.LogWarning("SprintSummaryReadModel for sprint {SprintId} not found when processing TaskCreatedEvent", notification.SprintId);
                return;
            }

            await RecalculateSprintMetricsAsync(readModel, ct);

            _logger.LogInformation("SprintSummaryReadModel updated successfully for task created in sprint: {SprintId}", notification.SprintId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SprintSummaryReadModel for task created in sprint: {SprintId}", notification.SprintId);
        }
    }

    public async System.Threading.Tasks.Task Handle(TaskUpdatedEvent notification, CancellationToken ct)
    {
        // Only update if task belongs to a sprint (check both old and new sprint IDs)
        var sprintId = notification.NewSprintId ?? notification.OldSprintId;
        if (!sprintId.HasValue)
            return;

        try
        {
            _logger.LogInformation("Updating SprintSummaryReadModel for task updated in sprint: {SprintId}", sprintId);

            // Update old sprint if task was moved
            if (notification.OldSprintId.HasValue && notification.OldSprintId != notification.NewSprintId)
            {
                var oldSprintReadModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
                    .Query()
                    .FirstOrDefaultAsync(r => r.SprintId == notification.OldSprintId.Value, ct);

                if (oldSprintReadModel != null)
                {
                    await RecalculateSprintMetricsAsync(oldSprintReadModel, ct);
                }
            }

            // Update new sprint
            if (notification.NewSprintId.HasValue)
            {
                var readModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
                    .Query()
                    .FirstOrDefaultAsync(r => r.SprintId == notification.NewSprintId.Value, ct);

                if (readModel != null)
                {
                    await RecalculateSprintMetricsAsync(readModel, ct);
                }
            }

            _logger.LogInformation("SprintSummaryReadModel updated successfully for task updated in sprint: {SprintId}", sprintId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SprintSummaryReadModel for task updated in sprint: {SprintId}", sprintId);
        }
    }

    private async System.Threading.Tasks.Task<SprintSummaryReadModel> GetOrCreateSprintSummaryReadModelAsync(int sprintId, int projectId, int organizationId, CancellationToken ct)
    {
        var readModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
            .Query()
            .FirstOrDefaultAsync(r => r.SprintId == sprintId, ct);

        if (readModel == null)
        {
            var sprint = await _unitOfWork.Repository<Sprint>()
                .GetByIdAsync(sprintId, ct);

            if (sprint == null)
            {
                throw new InvalidOperationException($"Sprint with ID {sprintId} not found");
            }

            readModel = new SprintSummaryReadModel
            {
                SprintId = sprintId,
                ProjectId = projectId,
                OrganizationId = organizationId,
                SprintName = $"Sprint {sprint.Number}",
                Status = sprint.Status,
                StartDate = sprint.StartDate ?? DateTimeOffset.UtcNow,
                EndDate = sprint.EndDate ?? DateTimeOffset.UtcNow.AddDays(14),
                LastUpdated = DateTimeOffset.UtcNow,
                Version = 1
            };

            await _unitOfWork.Repository<SprintSummaryReadModel>().AddAsync(readModel, ct);
        }

        return readModel;
    }

    private async System.Threading.Tasks.Task RecalculateSprintMetricsAsync(SprintSummaryReadModel readModel, CancellationToken ct)
    {
        // Get all tasks for this sprint
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var sprintTasks = await taskRepo.Query()
            .Where(t => t.SprintId == readModel.SprintId)
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

        await _unitOfWork.SaveChangesAsync(ct);
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

            // Calculate actual remaining points (completed tasks up to this date)
            var actualRemaining = totalStoryPoints;
            if (currentDate <= today)
            {
                // For past dates, calculate based on task completion dates
                // For simplicity, use current state (can be enhanced to track historical data)
                actualRemaining = readModel.TotalStoryPoints - readModel.CompletedStoryPoints;
            }

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
}

