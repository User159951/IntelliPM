using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when release notes are auto-generated for a release.
/// This event can trigger notifications, webhooks, or other downstream processes.
/// </summary>
public record ReleaseNotesGeneratedEvent : IDomainEvent, INotification
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
    /// The ID of the release for which notes were generated.
    /// </summary>
    public int ReleaseId { get; init; }

    /// <summary>
    /// The ID of the project the release belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The date and time when the notes were generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; init; }

    /// <summary>
    /// The ID of the user who triggered the generation.
    /// </summary>
    public int GeneratedBy { get; init; }
}
