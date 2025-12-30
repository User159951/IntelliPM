using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public class Notification : IAggregateRoot
{
    public int Id { get; set; }
    public int UserId { get; set; } // User who receives the notification
    public int OrganizationId { get; set; }
    public string Type { get; set; } = string.Empty; // task_assigned | task_completed | sprint_started | comment_added | project_invite
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; } // task | sprint | project | comment
    public int? EntityId { get; set; } // ID of the related entity
    public int? ProjectId { get; set; } // Optional: for filtering by project
    public bool IsRead { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public Project? Project { get; set; }
}
