namespace IntelliPM.Domain.Entities;

/// <summary>
/// Represents an outbox message for the Outbox pattern.
/// This entity stores domain events that need to be published reliably,
/// ensuring that events are persisted in the same transaction as the domain changes.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Unique identifier for the outbox message.
    /// </summary>
    public Guid Id { get; private set; }

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
    /// The date and time when the outbox message was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// The date and time when the outbox message was successfully processed.
    /// Null if the message has not been processed yet.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// The number of times processing has been attempted.
    /// Incremented on each retry attempt.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// The error message from the last failed processing attempt.
    /// Null if processing has never failed or if processing succeeded.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Idempotency key for preventing duplicate event processing.
    /// If multiple messages have the same IdempotencyKey and one is already processed,
    /// the others will be skipped to ensure idempotent behavior.
    /// </summary>
    public string? IdempotencyKey { get; private set; }

    /// <summary>
    /// The date and time when this message should be retried next.
    /// Null if the message should be processed immediately or has been processed.
    /// Used for exponential backoff retry logic.
    /// </summary>
    public DateTime? NextRetryAt { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// EF Core requires a parameterless constructor for materialization.
    /// </summary>
    private OutboxMessage()
    {
        // Required by EF Core
    }

    /// <summary>
    /// Creates a new outbox message for a domain event.
    /// </summary>
    /// <param name="eventType">The fully qualified type name of the domain event</param>
    /// <param name="payload">The JSON serialized representation of the domain event</param>
    /// <param name="idempotencyKey">Optional idempotency key to prevent duplicate processing. If not provided, one will be generated.</param>
    /// <returns>A new OutboxMessage instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when eventType or payload is null or empty</exception>
    public static OutboxMessage Create(string eventType, string payload, string? idempotencyKey = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentNullException(nameof(eventType), "Event type cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentNullException(nameof(payload), "Payload cannot be null or empty");
        }

        // Generate idempotency key if not provided
        var finalIdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? GenerateIdempotencyKey(eventType)
            : idempotencyKey;

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null,
            RetryCount = 0,
            Error = null,
            IdempotencyKey = finalIdempotencyKey,
            NextRetryAt = null
        };
    }

    /// <summary>
    /// Generates an idempotency key for a domain event.
    /// Pattern: EventTypeName_EntityId_TimestampTicks or EventTypeName_Guid if no entity ID available.
    /// </summary>
    /// <param name="eventType">The fully qualified type name of the domain event</param>
    /// <param name="entityId">Optional entity ID associated with the event</param>
    /// <returns>A unique idempotency key</returns>
    public static string GenerateIdempotencyKey(string eventType, string? entityId = null)
    {
        // Extract simple type name from fully qualified name
        var eventTypeName = eventType.Contains('.')
            ? eventType.Substring(eventType.LastIndexOf('.') + 1)
            : eventType;

        // Remove generic type parameters if present (e.g., "Event`1" -> "Event")
        if (eventTypeName.Contains('`'))
        {
            eventTypeName = eventTypeName.Substring(0, eventTypeName.IndexOf('`'));
        }

        var timestamp = DateTime.UtcNow.Ticks;

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            // Pattern: EventType_EntityId_TimestampTicks
            return $"{eventTypeName}_{entityId}_{timestamp}";
        }
        else
        {
            // Pattern: EventType_Guid_TimestampTicks (use GUID for uniqueness when no entity ID)
            return $"{eventTypeName}_{Guid.NewGuid()}_{timestamp}";
        }
    }

    /// <summary>
    /// Marks the outbox message as successfully processed.
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
        NextRetryAt = null;
    }

    /// <summary>
    /// Records a processing failure and increments the retry count.
    /// Sets NextRetryAt using exponential backoff: 2^RetryCount minutes.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure</param>
    public void RecordFailure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentNullException(nameof(errorMessage), "Error message cannot be null or empty");
        }

        RetryCount++;
        Error = errorMessage;
        ProcessedAt = null;

        // Exponential backoff: 2^RetryCount minutes
        // RetryCount 1: 2 minutes
        // RetryCount 2: 4 minutes
        // RetryCount 3: 8 minutes
        var backoffMinutes = Math.Pow(2, RetryCount);
        NextRetryAt = DateTime.UtcNow.AddMinutes(backoffMinutes);
    }

    /// <summary>
    /// Indicates whether the outbox message has been successfully processed.
    /// </summary>
    public bool IsProcessed => ProcessedAt.HasValue;

    /// <summary>
    /// Indicates whether the outbox message is pending processing.
    /// </summary>
    public bool IsPending => !ProcessedAt.HasValue;
}

