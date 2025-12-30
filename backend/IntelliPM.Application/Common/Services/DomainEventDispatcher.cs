using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Common.Services;

/// <summary>
/// Service for dispatching domain events to their registered handlers using MediatR.
/// Handles logging and exception handling for event dispatching.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Dispatches a single domain event to its registered handlers.
    /// </summary>
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent == null)
        {
            _logger.LogWarning("Attempted to dispatch a null domain event");
            return;
        }

        try
        {
            _logger.LogInformation(
                "Dispatching domain event: {EventType} with Id: {EventId} occurred at {OccurredOn}",
                domainEvent.GetType().Name,
                domainEvent.Id,
                domainEvent.OccurredOn);

            // Publish through MediatR (domain events should also implement INotification)
            // Using dynamic to handle the case where domain events implement INotification
            if (domainEvent is INotification notification)
            {
                await _mediator.Publish(notification, cancellationToken);
                _logger.LogInformation(
                    "Successfully dispatched domain event: {EventType} with Id: {EventId}",
                    domainEvent.GetType().Name,
                    domainEvent.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Domain event {EventType} with Id: {EventId} does not implement INotification. " +
                    "Domain events must implement both IDomainEvent and INotification to be dispatched.",
                    domainEvent.GetType().Name,
                    domainEvent.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error dispatching domain event: {EventType} with Id: {EventId}. Error: {ErrorMessage}",
                domainEvent.GetType().Name,
                domainEvent.Id,
                ex.Message);
            
            // Re-throw to allow caller to handle if needed
            // In some scenarios, you might want to swallow exceptions to prevent transaction rollback
            throw;
        }
    }

    /// <summary>
    /// Dispatches multiple domain events to their registered handlers.
    /// </summary>
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        if (domainEvents == null)
        {
            _logger.LogWarning("Attempted to dispatch a null collection of domain events");
            return;
        }

        var eventsList = domainEvents.ToList();
        
        if (eventsList.Count == 0)
        {
            _logger.LogDebug("No domain events to dispatch");
            return;
        }

        _logger.LogInformation(
            "Dispatching {EventCount} domain events",
            eventsList.Count);

        var tasks = eventsList.Select(domainEvent => 
            DispatchAsync(domainEvent, cancellationToken)).ToArray();

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation(
                "Successfully dispatched {EventCount} domain events",
                eventsList.Count);
        }
        catch (Exception ex)
        {
            var successCount = tasks.Count(t => t.IsCompletedSuccessfully);
            _logger.LogError(
                ex,
                "Error dispatching domain events batch. {SuccessCount}/{TotalCount} events were dispatched successfully. Error: {ErrorMessage}",
                successCount,
                eventsList.Count,
                ex.Message);
            
            // Re-throw to allow caller to handle if needed
            throw;
        }
    }
}

