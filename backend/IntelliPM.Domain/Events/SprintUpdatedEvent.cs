using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a sprint is updated in the system.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record SprintUpdatedEvent : IDomainEvent, INotification
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
    /// The ID of the sprint that was updated.
    /// </summary>
    public int SprintId { get; init; }

    /// <summary>
    /// The ID of the project the sprint belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// Dictionary of changes made to the sprint.
    /// Format: {"PropertyName": "OldValue -> NewValue"}
    /// </summary>
    public Dictionary<string, string> Changes { get; init; } = new();
}

