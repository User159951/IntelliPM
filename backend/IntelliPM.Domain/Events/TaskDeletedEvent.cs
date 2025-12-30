using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a task is deleted from the system.
/// This event is published via MediatR for projection handlers to update read models.
/// </summary>
public record TaskDeletedEvent : IDomainEvent, INotification
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
    /// The ID of the task that was deleted.
    /// </summary>
    public int TaskId { get; init; }

    /// <summary>
    /// The ID of the project the task belonged to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The ID of the sprint the task belonged to (if any).
    /// </summary>
    public int? SprintId { get; init; }
}

