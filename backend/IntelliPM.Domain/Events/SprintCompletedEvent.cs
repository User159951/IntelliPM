using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a sprint is completed.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record SprintCompletedEvent : IDomainEvent, INotification
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
    /// The ID of the sprint that was completed.
    /// </summary>
    public int SprintId { get; init; }

    /// <summary>
    /// The ID of the project the sprint belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The organization ID the sprint belongs to.
    /// </summary>
    public int OrganizationId { get; init; }

    /// <summary>
    /// The completed story points (velocity) for this sprint.
    /// </summary>
    public int CompletedStoryPoints { get; init; }
}

