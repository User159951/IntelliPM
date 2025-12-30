using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a user is mentioned in a comment.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record UserMentionedEvent : IDomainEvent, INotification
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
    /// The ID of the mention entity.
    /// </summary>
    public int MentionId { get; init; }

    /// <summary>
    /// The ID of the user who was mentioned.
    /// </summary>
    public int MentionedUserId { get; init; }

    /// <summary>
    /// The ID of the comment containing the mention.
    /// </summary>
    public int CommentId { get; init; }

    /// <summary>
    /// The ID of the user who wrote the comment.
    /// </summary>
    public int CommentAuthorId { get; init; }

    /// <summary>
    /// The name of the user who wrote the comment.
    /// </summary>
    public string CommentAuthorName { get; init; } = string.Empty;

    /// <summary>
    /// The type of entity the comment belongs to (Task, Project, Sprint, etc.).
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the entity the comment belongs to.
    /// </summary>
    public int EntityId { get; init; }

    /// <summary>
    /// The title or name of the entity (e.g., task title, project name).
    /// </summary>
    public string EntityTitle { get; init; } = string.Empty;

    /// <summary>
    /// The mention text (e.g., "@username").
    /// </summary>
    public string MentionText { get; init; } = string.Empty;

    /// <summary>
    /// The content of the comment containing the mention.
    /// </summary>
    public string CommentContent { get; init; } = string.Empty;

    /// <summary>
    /// The organization ID the mention belongs to.
    /// </summary>
    public int OrganizationId { get; init; }
}

