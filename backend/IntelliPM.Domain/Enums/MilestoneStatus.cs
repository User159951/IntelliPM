namespace IntelliPM.Domain.Enums;

/// <summary>
/// Status of a milestone in project management.
/// </summary>
public enum MilestoneStatus
{
    /// <summary>
    /// Pending - Milestone has not yet been reached or started.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// InProgress - Milestone is currently ongoing or in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Completed - Milestone has been successfully completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Missed - Milestone deadline has passed without completion.
    /// </summary>
    Missed = 3,

    /// <summary>
    /// Cancelled - Milestone has been cancelled and will not be completed.
    /// </summary>
    Cancelled = 4
}

