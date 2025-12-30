using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a comment is deleted (soft delete).
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record CommentDeletedEvent : IDomainEvent, INotification
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
    /// The ID of the comment that was deleted.
    /// </summary>
    public int CommentId { get; init; }

    /// <summary>
    /// The type of entity the comment belonged to (Task, Project, Sprint, etc.).
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the entity the comment belonged to.
    /// </summary>
    public int EntityId { get; init; }

    /// <summary>
    /// The organization ID the comment belonged to.
    /// </summary>
    public int OrganizationId { get; init; }
}

