namespace IntelliPM.Domain.Enums;

/// <summary>
/// Status of a release in the release management lifecycle.
/// </summary>
public enum ReleaseStatus
{
    /// <summary>
    /// Planned - Release is planned but not started.
    /// </summary>
    Planned = 0,

    /// <summary>
    /// InProgress - Release is being prepared.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Testing - Release is in testing phase.
    /// </summary>
    Testing = 2,

    /// <summary>
    /// ReadyForDeployment - Release is ready to deploy.
    /// </summary>
    ReadyForDeployment = 3,

    /// <summary>
    /// Deployed - Release has been deployed.
    /// </summary>
    Deployed = 4,

    /// <summary>
    /// Failed - Release deployment failed.
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Cancelled - Release was cancelled.
    /// </summary>
    Cancelled = 6
}

