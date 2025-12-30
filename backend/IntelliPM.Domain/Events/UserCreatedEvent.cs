using IntelliPM.Domain.Enums;
using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a new user is created in the system.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record UserCreatedEvent : IDomainEvent, INotification
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
    /// The ID of the newly created user.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// The username of the newly created user.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the newly created user.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The global role assigned to the newly created user.
    /// </summary>
    public GlobalRole Role { get; init; }

    /// <summary>
    /// The organization ID the user belongs to.
    /// </summary>
    public int OrganizationId { get; init; }

    /// <summary>
    /// The ID of the user who created this user (if applicable).
    /// Null if the user was self-registered.
    /// </summary>
    public int? CreatedById { get; init; }
}

