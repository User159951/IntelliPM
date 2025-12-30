using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a new project is created in the system.
/// This event is published via the Outbox pattern for reliable event processing.
/// </summary>
public record ProjectCreatedEvent : IDomainEvent, INotification
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
    /// The ID of the newly created project.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The organization ID the project belongs to.
    /// </summary>
    public int OrganizationId { get; init; }

    /// <summary>
    /// The project name.
    /// </summary>
    public string ProjectName { get; init; } = string.Empty;

    /// <summary>
    /// The project type (Scrum, Kanban).
    /// </summary>
    public string ProjectType { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the project owner.
    /// </summary>
    public int OwnerId { get; init; }

    /// <summary>
    /// The project status.
    /// </summary>
    public string Status { get; init; } = string.Empty;
}

