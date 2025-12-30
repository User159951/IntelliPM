using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when a task is updated in the system.
/// This event is published via MediatR for projection handlers to update read models.
/// </summary>
public record TaskUpdatedEvent : IDomainEvent, INotification
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
    /// The ID of the task that was updated.
    /// </summary>
    public int TaskId { get; init; }

    /// <summary>
    /// The ID of the project the task belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The previous status of the task (if changed).
    /// </summary>
    public string? OldStatus { get; init; }

    /// <summary>
    /// The new status of the task (if changed).
    /// </summary>
    public string? NewStatus { get; init; }

    /// <summary>
    /// The previous sprint ID of the task (if changed).
    /// </summary>
    public int? OldSprintId { get; init; }

    /// <summary>
    /// The new sprint ID of the task (if changed).
    /// </summary>
    public int? NewSprintId { get; init; }

    /// <summary>
    /// Dictionary of changes made to the task.
    /// Format: {"PropertyName": "OldValue -> NewValue"}
    /// Example: {"Priority": "Medium -> High", "StoryPoints": "3 -> 5"}
    /// </summary>
    public Dictionary<string, string> Changes { get; init; } = new();
}

