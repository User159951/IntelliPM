using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Represents a message that has been moved to the Dead Letter Queue (DLQ)
/// after exceeding maximum retry attempts in the Outbox pattern.
/// These messages require manual investigation and can be retried or deleted.
/// </summary>
public class DeadLetterMessage : IAggregateRoot
{
    /// <summary>
    /// Unique identifier for the dead letter message.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The original OutboxMessage ID that failed processing.
    /// </summary>
    public Guid OriginalMessageId { get; private set; }

    /// <summary>
    /// The fully qualified type name of the domain event.
    /// Used to deserialize the event from the payload.
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// JSON serialized representation of the domain event.
    /// Contains the complete event data needed for processing.
    /// </summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>
    /// The date and time when the original outbox message was created.
    /// </summary>
    public DateTime OriginalCreatedAt { get; private set; }

    /// <summary>
    /// The date and time when the message was moved to the DLQ.
    /// </summary>
    public DateTime MovedToDlqAt { get; private set; }

    /// <summary>
    /// The total number of retry attempts before moving to DLQ.
    /// </summary>
    public int TotalRetryAttempts { get; private set; }

    /// <summary>
    /// The error message from the last failed processing attempt.
    /// </summary>
    public string LastError { get; private set; } = string.Empty;

    /// <summary>
    /// Idempotency key from the original message for duplicate prevention.
    /// </summary>
    public string? IdempotencyKey { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// EF Core requires a parameterless constructor for materialization.
    /// </summary>
    private DeadLetterMessage()
    {
        // Required by EF Core
    }

    /// <summary>
    /// Creates a new dead letter message from a failed OutboxMessage.
    /// </summary>
    /// <param name="outboxMessage">The failed outbox message to move to DLQ</param>
    /// <returns>A new DeadLetterMessage instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when outboxMessage is null</exception>
    public static DeadLetterMessage CreateFromOutboxMessage(OutboxMessage outboxMessage)
    {
        if (outboxMessage == null)
        {
            throw new ArgumentNullException(nameof(outboxMessage), "Outbox message cannot be null");
        }

        return new DeadLetterMessage
        {
            Id = Guid.NewGuid(),
            OriginalMessageId = outboxMessage.Id,
            EventType = outboxMessage.EventType,
            Payload = outboxMessage.Payload,
            OriginalCreatedAt = outboxMessage.CreatedAt,
            MovedToDlqAt = DateTime.UtcNow,
            TotalRetryAttempts = outboxMessage.RetryCount,
            LastError = outboxMessage.Error ?? "Unknown error",
            IdempotencyKey = outboxMessage.IdempotencyKey
        };
    }
}

