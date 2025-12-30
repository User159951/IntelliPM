using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a comment is updated.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record CommentUpdatedEvent : IDomainEvent, INotification
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
    /// The ID of the comment that was updated.
    /// </summary>
    public int CommentId { get; init; }

    /// <summary>
    /// The ID of the user who updated the comment.
    /// </summary>
    public int AuthorId { get; init; }

    /// <summary>
    /// The type of entity the comment belongs to (Task, Project, Sprint, etc.).
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the entity the comment belongs to.
    /// </summary>
    public int EntityId { get; init; }

    /// <summary>
    /// The previous content of the comment.
    /// </summary>
    public string OldContent { get; init; } = string.Empty;

    /// <summary>
    /// The new content of the comment.
    /// </summary>
    public string NewContent { get; init; } = string.Empty;

    /// <summary>
    /// The organization ID the comment belongs to.
    /// </summary>
    public int OrganizationId { get; init; }
}

