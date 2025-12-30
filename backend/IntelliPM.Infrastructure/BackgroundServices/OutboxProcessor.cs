using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Events;
using IntelliPM.Domain.Entities;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using OutboxMessage = IntelliPM.Domain.Entities.OutboxMessage;

namespace IntelliPM.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that processes outbox messages from the database.
/// Implements the Outbox pattern to ensure reliable event publishing.
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private const int PollingIntervalSeconds = 5;
    private const int MaxRetryAttempts = 3;

    public OutboxProcessor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OutboxProcessor main loop. Will retry in {Interval} seconds", PollingIntervalSeconds);
            }

            // Wait for the polling interval before next iteration
            // Use cancellation token properly to allow graceful shutdown
            try
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(PollingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested - break the loop gracefully
                _logger.LogInformation("OutboxProcessor cancellation requested, shutting down gracefully");
                break;
            }
        }

        _logger.LogInformation("OutboxProcessor background service stopped");
    }

    private async System.Threading.Tasks.Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        // Fetch unprocessed messages where:
        // - ProcessedAt is null (not yet processed)
        // - RetryCount < MaxRetryAttempts (hasn't exceeded max retries)
        // - NextRetryAt is null (first attempt) OR NextRetryAt <= now (ready for retry)
        var now = DateTime.UtcNow;
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetryAttempts)
            .Where(m => m.NextRetryAt == null || m.NextRetryAt <= now)
            .OrderBy(m => m.CreatedAt)
            .Take(10) // Process in batches to avoid overwhelming the system
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            _logger.LogDebug("No outbox messages to process");
            return;
        }

        _logger.LogInformation(
            "Found {Count} outbox message(s) ready for processing",
            messages.Count);

        foreach (var message in messages)
        {
            try
            {
                // Check idempotency before processing
                if (!string.IsNullOrWhiteSpace(message.IdempotencyKey))
                {
                    var alreadyProcessed = await CheckIdempotencyAsync(
                        message.IdempotencyKey,
                        message.Id,
                        dbContext,
                        cancellationToken);

                    if (alreadyProcessed)
                    {
                        _logger.LogInformation(
                            "Skipping duplicate message with IdempotencyKey: {IdempotencyKey}",
                            message.IdempotencyKey);

                        // Mark as processed since a duplicate was already processed
                        message.MarkAsProcessed();
                        await dbContext.SaveChangesAsync(cancellationToken);
                        continue;
                    }
                }

                await ProcessMessageAsync(message, dbContext, dispatcher, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId} (Attempt {RetryCount}/{MaxRetries}). Error: {ErrorMessage}",
                    message.Id,
                    message.RetryCount + 1,
                    MaxRetryAttempts,
                    ex.Message);

                // Record failure and set NextRetryAt using exponential backoff
                try
                {
                    message.RecordFailure(ex.Message);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    if (message.RetryCount < MaxRetryAttempts)
                    {
                        _logger.LogWarning(
                            "Outbox message {MessageId} will be retried at {NextRetryAt} (Attempt {RetryCount}/{MaxRetries})",
                            message.Id,
                            message.NextRetryAt,
                            message.RetryCount,
                            MaxRetryAttempts);
                    }
                    else
                    {
                        // Move to Dead Letter Queue after max retries
                        await MoveToDeadLetterQueueAsync(message, dbContext, cancellationToken);
                    }
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(
                        saveEx,
                        "Failed to save failure state for outbox message {MessageId}",
                        message.Id);
                }
            }
        }
    }

    private async System.Threading.Tasks.Task<bool> CheckIdempotencyAsync(
        string idempotencyKey,
        Guid currentMessageId,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Check if another message with the same IdempotencyKey was already processed
        var processedMessage = await dbContext.OutboxMessages
            .Where(m => m.IdempotencyKey == idempotencyKey && m.Id != currentMessageId && m.ProcessedAt != null)
            .FirstOrDefaultAsync(cancellationToken);

        return processedMessage != null;
    }

    private async System.Threading.Tasks.Task ProcessMessageAsync(
        OutboxMessage message,
        AppDbContext dbContext,
        IDomainEventDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var attemptNumber = message.RetryCount + 1;
        var isRetry = message.RetryCount > 0;

        if (isRetry)
        {
            _logger.LogInformation(
                "Retrying outbox message {MessageId} of type {EventType} (Retry Attempt {RetryCount}/{MaxRetries})",
                message.Id,
                message.EventType,
                attemptNumber,
                MaxRetryAttempts);
        }
        else
        {
            _logger.LogInformation(
                "Processing outbox message {MessageId} of type {EventType} (Initial Attempt)",
                message.Id,
                message.EventType);
        }

        // Deserialize the domain event from JSON payload
        var domainEvent = DeserializeDomainEvent(message.EventType, message.Payload);
        if (domainEvent == null)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize domain event of type {message.EventType}");
        }

        // Dispatch the event
        await dispatcher.DispatchAsync(domainEvent, cancellationToken);

        // Mark as processed on success
        message.MarkAsProcessed();
        await dbContext.SaveChangesAsync(cancellationToken);

        if (isRetry)
        {
            _logger.LogInformation(
                "Successfully processed outbox message {MessageId} of type {EventType} after {RetryCount} retry attempt(s)",
                message.Id,
                message.EventType,
                message.RetryCount);
        }
        else
        {
            _logger.LogInformation(
                "Successfully processed outbox message {MessageId} of type {EventType}",
                message.Id,
                message.EventType);
        }
    }

    private IDomainEvent? DeserializeDomainEvent(string eventType, string payload)
    {
        try
        {
            // Get the type from the fully qualified name
            var type = Type.GetType(eventType);
            if (type == null)
            {
                _logger.LogWarning(
                    "Could not find type {EventType} for deserialization",
                    eventType);
                return null;
            }

            // Check if the type implements IDomainEvent
            if (!typeof(IDomainEvent).IsAssignableFrom(type))
            {
                _logger.LogWarning(
                    "Type {EventType} does not implement IDomainEvent",
                    eventType);
                return null;
            }

            // Deserialize using System.Text.Json
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var domainEvent = JsonSerializer.Deserialize(payload, type, options) as IDomainEvent;

            if (domainEvent == null)
            {
                _logger.LogWarning(
                    "Deserialization of {EventType} returned null",
                    eventType);
            }

            return domainEvent;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "JSON deserialization error for event type {EventType}. Payload: {Payload}",
                eventType,
                payload);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error deserializing event type {EventType}",
                eventType);
            return null;
        }
    }

    /// <summary>
    /// Moves a failed outbox message to the Dead Letter Queue.
    /// </summary>
    private async System.Threading.Tasks.Task MoveToDeadLetterQueueAsync(
        OutboxMessage message,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create dead letter message from failed outbox message
            var deadLetterMessage = DeadLetterMessage.CreateFromOutboxMessage(message);

            // Add to DLQ
            dbContext.DeadLetterMessages.Add(deadLetterMessage);

            // Remove from outbox
            dbContext.OutboxMessages.Remove(message);

            // Save changes in a single transaction
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Message {MessageId} moved to DLQ after {RetryCount} failed attempts. DLQ ID: {DlqId}",
                message.Id,
                message.RetryCount,
                deadLetterMessage.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to move message {MessageId} to DLQ. Error: {ErrorMessage}",
                message.Id,
                ex.Message);
            
            // Re-throw to allow caller to handle
            throw;
        }
    }
}

