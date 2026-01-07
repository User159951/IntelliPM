namespace IntelliPM.Domain.Enums;

/// <summary>
/// Status values for AI decision approval workflow.
/// </summary>
public enum AIDecisionStatus
{
    /// <summary>
    /// Decision is pending approval or execution.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Decision has been approved by an authorized user.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Decision has been rejected by an authorized user.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Decision has been applied/executed.
    /// </summary>
    Applied = 3,

    /// <summary>
    /// Decision expired after approval deadline (48 hours).
    /// </summary>
    Expired = 4
}

