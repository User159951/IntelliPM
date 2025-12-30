using MediatR;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Command to add a comment to an entity (Task, Project, Sprint, etc.).
/// </summary>
public record AddCommentCommand : IRequest<AddCommentResponse>
{
    /// <summary>
    /// The type of entity the comment is being added to (Task, Project, Sprint, Defect, BacklogItem).
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the entity the comment is being added to.
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
}

/// <summary>
/// Response containing the created comment information.
/// </summary>
public record AddCommentResponse(
    int CommentId,
    int AuthorId,
    string AuthorName,
    string Content,
    DateTimeOffset CreatedAt,
    List<int> MentionedUserIds
);

