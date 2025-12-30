namespace IntelliPM.Application.Features.Releases.DTOs;

/// <summary>
/// Data Transfer Object for QualityGate entity.
/// Contains quality gate information for API responses.
/// </summary>
public class QualityGateDto
{
    /// <summary>
    /// Unique identifier for the quality gate.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the release this quality gate belongs to.
    /// </summary>
    public int ReleaseId { get; set; }

    /// <summary>
    /// Type of quality gate as string (e.g., "AllTasksCompleted", "NoOpenBugs").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Status of the quality gate as string (e.g., "Passed", "Failed", "Pending").
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this quality gate is required for deployment.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Optional threshold value for this gate (e.g., 80 for 80%).
    /// </summary>
    public decimal? Threshold { get; set; }

    /// <summary>
    /// Optional actual measured value.
    /// </summary>
    public decimal? ActualValue { get; set; }

    /// <summary>
    /// Message explaining the quality gate result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional detailed information about the quality gate result.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Date and time when the quality gate was checked.
    /// Null if not yet checked.
    /// </summary>
    public DateTimeOffset? CheckedAt { get; set; }

    /// <summary>
    /// Username of the user who performed the check.
    /// Null if checked by system or not yet checked.
    /// </summary>
    public string? CheckedByName { get; set; }
}

