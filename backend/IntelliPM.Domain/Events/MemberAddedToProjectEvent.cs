using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a member is added to a project.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record MemberAddedToProjectEvent : IDomainEvent, INotification
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
    /// The ID of the project the member was added to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The organization ID the project belongs to.
    /// </summary>
    public int OrganizationId { get; init; }

    /// <summary>
    /// The ID of the user that was added.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// The role assigned to the member.
    /// </summary>
    public string Role { get; init; } = string.Empty;
}

