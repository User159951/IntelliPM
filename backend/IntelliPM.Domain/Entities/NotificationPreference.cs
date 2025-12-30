using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Notification preference entity for user notification settings.
/// Allows users to configure which notification types they want to receive and through which channels.
/// </summary>
public class NotificationPreference : IAggregateRoot
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy

    // Notification type
    public string NotificationType { get; set; } = string.Empty; // "TaskAssigned", "Mention", etc.

    // Channel preferences
    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = false; // Future: push notifications

    // Frequency settings
    public string Frequency { get; set; } = "Instant"; // "Instant", "Daily", "Weekly", "Never"

    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}

