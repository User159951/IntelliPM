using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a new sprint is created in the system.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record SprintCreatedEvent : IDomainEvent, INotification
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
    /// The ID of the newly created sprint.
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
    /// The sprint number.
    /// </summary>
    public int Number { get; init; }

    /// <summary>
    /// The sprint goal.
    /// </summary>
    public string Goal { get; init; } = string.Empty;

    /// <summary>
    /// The sprint start date.
    /// </summary>
    public DateTimeOffset? StartDate { get; init; }

    /// <summary>
    /// The sprint end date.
    /// </summary>
    public DateTimeOffset? EndDate { get; init; }

    /// <summary>
    /// The sprint status.
    /// </summary>
    public string Status { get; init; } = string.Empty;
}

