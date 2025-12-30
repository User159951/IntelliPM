using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a milestone is marked as completed.
/// This event is published via MediatR for projection handlers to update read models.
/// </summary>
public record MilestoneCompletedEvent : IDomainEvent, INotification
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
    /// The ID of the completed milestone.
    /// </summary>
    public int MilestoneId { get; init; }

    /// <summary>
    /// The ID of the project the milestone belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The date and time when the milestone was completed.
    /// </summary>
    public DateTimeOffset CompletedAt { get; init; }
}

