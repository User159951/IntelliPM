using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a member is removed from a project.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record MemberRemovedFromProjectEvent : IDomainEvent, INotification
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
    /// The ID of the project the member was removed from.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The organization ID the project belongs to.
    /// </summary>
    public int OrganizationId { get; init; }

    /// <summary>
    /// The ID of the user that was removed.
    /// </summary>
    public int UserId { get; init; }
}

