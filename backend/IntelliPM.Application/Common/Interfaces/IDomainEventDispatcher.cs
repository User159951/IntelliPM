using IntelliPM.Domain.Events;

namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for dispatching domain events to their registered handlers.
/// Domain events are published through MediatR to trigger side effects and maintain eventual consistency.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a single domain event to its registered handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches multiple domain events to their registered handlers.
    /// </summary>
    /// <param name="domainEvents">The collection of domain events to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

