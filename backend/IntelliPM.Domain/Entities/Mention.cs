using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Mention entity for tracking user mentions (@username) in comments.
/// Tracks which users are mentioned in which comments and notification delivery status.
/// </summary>
public class Mention : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy

    // Comment reference
    public int CommentId { get; set; }

    // Mentioned user
    public int MentionedUserId { get; set; }

    // Mention metadata
    public int StartIndex { get; set; } // Position in comment content
    public int Length { get; set; } // Length of mention text (@username)
    public string MentionText { get; set; } = string.Empty; // e.g., "@john.doe"

    // Notification tracking
    public bool NotificationSent { get; set; } = false;
    public DateTimeOffset? NotificationSentAt { get; set; }

    // Metadata
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Comment Comment { get; set; } = null!;
    public User MentionedUser { get; set; } = null!;
}

