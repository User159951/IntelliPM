namespace IntelliPM.Domain.Events;

/// <summary>
/// Represents a domain event that occurred in the system.
/// Domain events are used to trigger side effects and maintain eventual consistency.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The date and time when this event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}

