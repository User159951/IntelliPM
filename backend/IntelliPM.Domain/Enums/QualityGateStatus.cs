namespace IntelliPM.Domain.Enums;

/// <summary>
/// Status of a quality gate evaluation.
/// </summary>
public enum QualityGateStatus
{
    /// <summary>
    /// Passed - All quality checks passed.
    /// </summary>
    Passed = 0,

    /// <summary>
    /// Warning - Some warnings but can proceed.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Failed - Critical issues, cannot deploy.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Pending - Not yet evaluated.
    /// </summary>
    Pending = 3,

    /// <summary>
    /// Skipped - Quality gate was skipped.
    /// </summary>
    Skipped = 4
}

