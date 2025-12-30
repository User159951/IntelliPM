using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a new task is created in the system.
/// This event is published via MediatR for projection handlers to update read models.
/// </summary>
public record TaskCreatedEvent : IDomainEvent, INotification
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
    /// The ID of the newly created task.
    /// </summary>
    public int TaskId { get; init; }

    /// <summary>
    /// The ID of the project the task belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The title of the task.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The status of the task (Todo, InProgress, Done, etc.).
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The priority of the task (Low, Medium, High, Critical).
    /// </summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>
    /// The story points assigned to the task.
    /// </summary>
    public int? StoryPoints { get; init; }

    /// <summary>
    /// The ID of the sprint the task belongs to (if any).
    /// </summary>
    public int? SprintId { get; init; }
}

