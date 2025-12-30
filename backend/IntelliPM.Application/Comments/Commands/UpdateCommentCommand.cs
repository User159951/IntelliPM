using IntelliPM.Application.Comments.Queries;
using MediatR;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Command to update an existing comment.
/// </summary>
public record UpdateCommentCommand : IRequest<CommentDto>
{
    /// <summary>
    /// The ID of the comment to update.
    /// </summary>
    public int CommentId { get; init; }

    /// <summary>
    /// The new content for the comment.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the user making the update (for authorization).
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// The organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; init; }
}

