using MediatR;

namespace IntelliPM.Application.Comments.Queries;

/// <summary>
/// Query to retrieve all comments for a specific entity.
/// </summary>
public class GetCommentsQuery : IRequest<List<CommentDto>>
{
    /// <summary>
    /// The type of entity to get comments for (Task, Project, Sprint, Defect, BacklogItem).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the entity to get comments for.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// The organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; set; }
}

/// <summary>
/// DTO representing a comment for API responses.
/// </summary>
public class CommentDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsEdited { get; set; }
    public int? ParentCommentId { get; set; }
}

