using IntelliPM.Domain.Enums;

namespace IntelliPM.Domain.ValueObjects;

/// <summary>
/// Value object representing the result of a quality gate evaluation.
/// Immutable result that can be used to create or update QualityGate entities.
/// </summary>
public record QualityGateResult
{
    /// <summary>
    /// Type of quality gate that was evaluated.
    /// </summary>
    public QualityGateType GateType { get; init; }

    /// <summary>
    /// Status of the quality gate evaluation.
    /// </summary>
    public QualityGateStatus Status { get; init; }

    /// <summary>
    /// Message explaining the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Optional detailed information about the result.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Date and time when the check was performed.
    /// </summary>
    public DateTimeOffset CheckedAt { get; init; }

    /// <summary>
    /// User or system who performed the check.
    /// </summary>
    public string? CheckedBy { get; init; }

    /// <summary>
    /// Optional threshold value for this gate (e.g., 80 for 80%).
    /// </summary>
    public decimal? Threshold { get; init; }

    /// <summary>
    /// Optional actual measured value.
    /// </summary>
    public decimal? ActualValue { get; init; }

    private QualityGateResult()
    {
        // Private constructor for record
    }

    /// <summary>
    /// Creates a successful quality gate result.
    /// </summary>
    /// <param name="gateType">Type of quality gate.</param>
    /// <param name="message">Success message.</param>
    /// <param name="threshold">Optional threshold value.</param>
    /// <param name="actualValue">Optional actual value.</param>
    /// <returns>QualityGateResult with Passed status.</returns>
    public static QualityGateResult Success(
        QualityGateType gateType,
        string message,
        decimal? threshold = null,
        decimal? actualValue = null)
    {
        return new QualityGateResult
        {
            GateType = gateType,
            Status = QualityGateStatus.Passed,
            Message = message,
            CheckedAt = DateTimeOffset.UtcNow,
            Threshold = threshold,
            ActualValue = actualValue
        };
    }

    /// <summary>
    /// Creates a warning quality gate result.
    /// </summary>
    /// <param name="gateType">Type of quality gate.</param>
    /// <param name="message">Warning message.</param>
    /// <param name="details">Optional detailed information.</param>
    /// <param name="threshold">Optional threshold value.</param>
    /// <param name="actualValue">Optional actual value.</param>
    /// <returns>QualityGateResult with Warning status.</returns>
    public static QualityGateResult Warning(
        QualityGateType gateType,
        string message,
        string? details = null,
        decimal? threshold = null,
        decimal? actualValue = null)
    {
        return new QualityGateResult
        {
            GateType = gateType,
            Status = QualityGateStatus.Warning,
            Message = message,
            Details = details,
            CheckedAt = DateTimeOffset.UtcNow,
            Threshold = threshold,
            ActualValue = actualValue
        };
    }

    /// <summary>
    /// Creates a failed quality gate result.
    /// </summary>
    /// <param name="gateType">Type of quality gate.</param>
    /// <param name="message">Failure message.</param>
    /// <param name="details">Optional detailed information.</param>
    /// <param name="threshold">Optional threshold value.</param>
    /// <param name="actualValue">Optional actual value.</param>
    /// <returns>QualityGateResult with Failed status.</returns>
    public static QualityGateResult Failed(
        QualityGateType gateType,
        string message,
        string? details = null,
        decimal? threshold = null,
        decimal? actualValue = null)
    {
        return new QualityGateResult
        {
            GateType = gateType,
            Status = QualityGateStatus.Failed,
            Message = message,
            Details = details,
            CheckedAt = DateTimeOffset.UtcNow,
            Threshold = threshold,
            ActualValue = actualValue
        };
    }

    /// <summary>
    /// Creates a pending quality gate result.
    /// </summary>
    /// <param name="gateType">Type of quality gate.</param>
    /// <param name="message">Pending message.</param>
    /// <returns>QualityGateResult with Pending status.</returns>
    public static QualityGateResult Pending(
        QualityGateType gateType,
        string message)
    {
        return new QualityGateResult
        {
            GateType = gateType,
            Status = QualityGateStatus.Pending,
            Message = message,
            CheckedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a skipped quality gate result.
    /// </summary>
    /// <param name="gateType">Type of quality gate.</param>
    /// <param name="message">Skip message.</param>
    /// <returns>QualityGateResult with Skipped status.</returns>
    public static QualityGateResult Skipped(
        QualityGateType gateType,
        string message)
    {
        return new QualityGateResult
        {
            GateType = gateType,
            Status = QualityGateStatus.Skipped,
            Message = message,
            CheckedAt = DateTimeOffset.UtcNow
        };
    }
}

