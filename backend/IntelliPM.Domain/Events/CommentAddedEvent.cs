using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a comment is added to an entity.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record CommentAddedEvent : IDomainEvent, INotification
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
    /// The ID of the comment that was added.
    /// </summary>
    public int CommentId { get; init; }

    /// <summary>
    /// The ID of the user who added the comment.
    /// </summary>
    public int AuthorId { get; init; }

    /// <summary>
    /// The name of the user who added the comment.
    /// </summary>
    public string AuthorName { get; init; } = string.Empty;

    /// <summary>
    /// The type of entity the comment was added to (Task, Project, Sprint, etc.).
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the entity the comment was added to.
    /// </summary>
    public int EntityId { get; init; }

    /// <summary>
    /// The content of the comment.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the parent comment if this is a reply (null for top-level comments).
    /// </summary>
    public int? ParentCommentId { get; init; }

    /// <summary>
    /// The organization ID the comment belongs to.
    /// </summary>
    public int OrganizationId { get; init; }
}

