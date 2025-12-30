using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a new milestone is created in the system.
/// This event is published via MediatR for projection handlers to update read models.
/// </summary>
public record MilestoneCreatedEvent : IDomainEvent, INotification
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
    /// The ID of the newly created milestone.
    /// </summary>
    public int MilestoneId { get; init; }

    /// <summary>
    /// The ID of the project the milestone belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The name of the milestone.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The due date of the milestone.
    /// </summary>
    public DateTimeOffset DueDate { get; init; }

    /// <summary>
    /// The type of milestone (Release, Sprint, Deadline, Custom).
    /// </summary>
    public int Type { get; init; }
}

