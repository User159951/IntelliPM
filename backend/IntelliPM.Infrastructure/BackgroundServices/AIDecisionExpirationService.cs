using IntelliPM.Application.AI.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that automatically expires pending AI decisions that have passed their approval deadline (48 hours).
/// Runs periodically to mark expired decisions.
/// </summary>
public class AIDecisionExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AIDecisionExpirationService> _logger;
    private const int CheckIntervalHours = 1; // Check every hour

    public AIDecisionExpirationService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AIDecisionExpirationService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AIDecisionExpirationService background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireOldDecisionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AIDecisionExpirationService main loop. Will retry in {Interval} hours", CheckIntervalHours);
            }

            // Wait for the check interval before next iteration
            try
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromHours(CheckIntervalHours), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested - break the loop gracefully
                _logger.LogInformation("AIDecisionExpirationService cancellation requested, shutting down gracefully");
                break;
            }
        }

        _logger.LogInformation("AIDecisionExpirationService background service stopped");
    }

    private async System.Threading.Tasks.Task ExpireOldDecisionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new ExpireOldDecisionsCommand
        {
            OrganizationId = null, // Process all organizations
            BatchSize = 100 // Process up to 100 decisions per run
        };

        var result = await mediator.Send(command, cancellationToken);

        if (result.ExpiredCount > 0)
        {
            _logger.LogInformation(
                "Expired {Count} AI decisions that passed their approval deadline. Decision IDs: {DecisionIds}",
                result.ExpiredCount,
                string.Join(", ", result.ExpiredDecisionIds));
        }
        else
        {
            _logger.LogDebug("No expired AI decisions found");
        }
    }
}

