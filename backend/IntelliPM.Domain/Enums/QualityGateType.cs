namespace IntelliPM.Domain.Enums;

/// <summary>
/// Type of quality gate check.
/// </summary>
public enum QualityGateType
{
    /// <summary>
    /// CodeCoverage - Unit test coverage check.
    /// </summary>
    CodeCoverage = 0,

    /// <summary>
    /// AllTasksCompleted - All tasks must be done.
    /// </summary>
    AllTasksCompleted = 1,

    /// <summary>
    /// NoOpenBugs - No critical/high priority bugs open.
    /// </summary>
    NoOpenBugs = 2,

    /// <summary>
    /// CodeReviewApproval - All code reviews approved.
    /// </summary>
    CodeReviewApproval = 3,

    /// <summary>
    /// SecurityScan - Security vulnerabilities check.
    /// </summary>
    SecurityScan = 4,

    /// <summary>
    /// PerformanceTests - Performance benchmarks met.
    /// </summary>
    PerformanceTests = 5,

    /// <summary>
    /// DocumentationComplete - Documentation is up to date.
    /// </summary>
    DocumentationComplete = 6,

    /// <summary>
    /// ManualApproval - Requires manual approval from stakeholder.
    /// </summary>
    ManualApproval = 7
}

