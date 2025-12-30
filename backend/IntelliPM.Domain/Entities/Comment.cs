using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Comment entity for adding comments to tasks, projects, sprints, defects, and other entities.
/// Uses polymorphic pattern with EntityType and EntityId for flexible association.
/// Supports comment threading with ParentCommentId for nested replies.
/// </summary>
public class Comment : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy

    // Polymorphic relationship
    public string EntityType { get; set; } = string.Empty; // "Task", "Project", "Sprint", "Defect", "BacklogItem"
    public int EntityId { get; set; } // ID of the entity being commented on

    // Comment content
    public string Content { get; set; } = string.Empty; // Rich text or markdown

    // Author information
    public int AuthorId { get; set; }

    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; } // Soft delete
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    // Parent comment for threading (optional)
    public int? ParentCommentId { get; set; }

    // Navigation properties
    public User Author { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<Mention> Mentions { get; set; } = new List<Mention>();
}

