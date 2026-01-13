using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Quality gate entity representing a quality check for a release.
/// Quality gates validate release readiness before deployment.
/// </summary>
public class QualityGate : IAggregateRoot, ITenantEntity
{
    /// <summary>
    /// Unique identifier for the quality gate.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; set; }

    /// <summary>
    /// ID of the release this quality gate belongs to.
    /// </summary>
    public int ReleaseId { get; set; }

    /// <summary>
    /// Type of quality gate check.
    /// </summary>
    public QualityGateType Type { get; set; }

    /// <summary>
    /// Status of the quality gate evaluation.
    /// Default: Pending.
    /// </summary>
    public QualityGateStatus Status { get; set; } = QualityGateStatus.Pending;

    /// <summary>
    /// Indicates if this quality gate is required for deployment.
    /// If false, only a warning is issued if it fails.
    /// Default: true.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Indicates if this quality gate is blocking (must pass before deployment).
    /// If true and the gate fails, deployment is blocked.
    /// If false, the gate can fail but deployment can still proceed (with warning).
    /// Default: true (blocking by default for safety).
    /// </summary>
    public bool IsBlocking { get; set; } = true;

    /// <summary>
    /// Optional threshold value for this gate (e.g., 80 for 80% code coverage).
    /// </summary>
    public decimal? Threshold { get; set; }

    /// <summary>
    /// Optional actual measured value (e.g., actual code coverage percentage).
    /// </summary>
    public decimal? ActualValue { get; set; }

    /// <summary>
    /// Message explaining the quality gate result.
    /// Maximum length: 500 characters.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional detailed information about the quality gate result.
    /// Maximum length: 2000 characters.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Date and time when the quality gate was checked.
    /// Null if not yet checked.
    /// </summary>
    public DateTimeOffset? CheckedAt { get; set; }

    /// <summary>
    /// ID of the user who performed the check.
    /// Null if checked by system or not yet checked.
    /// </summary>
    public int? CheckedByUserId { get; set; }

    /// <summary>
    /// Date and time when the quality gate was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties

    /// <summary>
    /// Release this quality gate belongs to.
    /// </summary>
    public Release Release { get; set; } = null!;

    /// <summary>
    /// User who performed the check.
    /// Null if checked by system or not yet checked.
    /// </summary>
    public User? CheckedByUser { get; set; }

    /// <summary>
    /// Organization this quality gate belongs to.
    /// </summary>
    public Organization Organization { get; set; } = null!;
}

