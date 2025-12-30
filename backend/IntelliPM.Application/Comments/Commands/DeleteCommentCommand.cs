using MediatR;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Command to delete (soft delete) a comment.
/// </summary>
public record DeleteCommentCommand : IRequest<Unit>
{
    /// <summary>
    /// The ID of the comment to delete.
    /// </summary>
    public int CommentId { get; init; }

    /// <summary>
    /// The ID of the user making the deletion (for authorization).
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// The organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; init; }
}

