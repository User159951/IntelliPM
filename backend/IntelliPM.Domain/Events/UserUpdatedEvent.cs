using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a user is updated in the system.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record UserUpdatedEvent : IDomainEvent, INotification
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
    /// The ID of the user that was updated.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// The username of the updated user.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the updated user.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The organization ID the user belongs to.
    /// </summary>
    public int OrganizationId { get; init; }

    /// <summary>
    /// The ID of the user who performed the update.
    /// </summary>
    public int UpdatedById { get; init; }

    /// <summary>
    /// Dictionary of changes made to the user.
    /// Format: {"PropertyName": "OldValue -> NewValue"}
    /// Example: {"IsActive": "false -> true", "Role": "User -> Admin"}
    /// </summary>
    public Dictionary<string, string> Changes { get; init; } = new();
}

