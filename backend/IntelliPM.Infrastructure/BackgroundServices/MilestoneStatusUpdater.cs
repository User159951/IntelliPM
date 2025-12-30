using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Events;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;

namespace IntelliPM.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that automatically updates milestone statuses.
/// Runs periodically to mark overdue milestones as missed.
/// </summary>
public class MilestoneStatusUpdater : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MilestoneStatusUpdater> _logger;
    private const int CheckIntervalHours = 1; // Check every hour

    public MilestoneStatusUpdater(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MilestoneStatusUpdater> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MilestoneStatusUpdater background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateOverdueMilestonesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MilestoneStatusUpdater main loop. Will retry in {Interval} hours", CheckIntervalHours);
            }

            // Wait for the check interval before next iteration
            try
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromHours(CheckIntervalHours), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested - break the loop gracefully
                _logger.LogInformation("MilestoneStatusUpdater cancellation requested, shutting down gracefully");
                break;
            }
        }

        _logger.LogInformation("MilestoneStatusUpdater background service stopped");
    }

    private async System.Threading.Tasks.Task UpdateOverdueMilestonesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var now = DateTimeOffset.UtcNow;

        // Find milestones that are overdue and still pending or in progress
        var overdueMilestones = await dbContext.Milestones
            .Where(m => m.DueDate < now)
            .Where(m => m.Status == MilestoneStatus.Pending || m.Status == MilestoneStatus.InProgress)
            .ToListAsync(cancellationToken);

        if (overdueMilestones.Count == 0)
        {
            _logger.LogDebug("No overdue milestones to update");
            return;
        }

        _logger.LogInformation(
            "Found {Count} overdue milestone(s) to mark as missed",
            overdueMilestones.Count);

        foreach (var milestone in overdueMilestones)
        {
            try
            {
                // Use domain method to mark as missed (includes validation)
                milestone.MarkAsMissed();

                // Publish domain event
                var missedEvent = new MilestoneMissedEvent
                {
                    MilestoneId = milestone.Id,
                    ProjectId = milestone.ProjectId,
                    DueDate = milestone.DueDate
                };

                await mediator.Publish(missedEvent, cancellationToken);

                _logger.LogInformation(
                    "Marked milestone {MilestoneId} ({Name}) as missed. Due date: {DueDate}",
                    milestone.Id,
                    milestone.Name,
                    milestone.DueDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to mark milestone {MilestoneId} as missed. Error: {ErrorMessage}",
                    milestone.Id,
                    ex.Message);
            }
        }

        // Save all changes
        try
        {
            var savedCount = await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Successfully updated {Count} milestone(s) status",
                savedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save milestone status updates. Error: {ErrorMessage}",
                ex.Message);
        }
    }
}

