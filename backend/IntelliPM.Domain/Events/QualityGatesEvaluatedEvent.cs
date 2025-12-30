using MediatR;

namespace IntelliPM.Domain.Events;

/// <summary>
/// Domain event raised when quality gates are evaluated for a release.
/// This event can trigger notifications, webhooks, or other downstream processes.
/// </summary>
public record QualityGatesEvaluatedEvent : IDomainEvent, INotification
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
    /// The ID of the release for which quality gates were evaluated.
    /// </summary>
    public int ReleaseId { get; init; }

    /// <summary>
    /// The ID of the project the release belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The overall quality gate status after evaluation.
    /// </summary>
    public string OverallStatus { get; init; } = string.Empty;

    /// <summary>
    /// The date and time when the quality gates were evaluated.
    /// </summary>
    public DateTimeOffset EvaluatedAt { get; init; }

    /// <summary>
    /// The ID of the user who triggered the evaluation.
    /// </summary>
    public int EvaluatedBy { get; init; }
}

