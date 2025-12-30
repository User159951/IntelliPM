using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a project is updated in the system.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record ProjectUpdatedEvent : IDomainEvent, INotification
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// The date and time when this event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// The ID of the project that was updated.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// Dictionary of changes made to the project.
    /// Format: {"PropertyName": "OldValue -> NewValue"}
    /// </summary>
    public Dictionary<string, string> Changes { get; init; } = new();
}

