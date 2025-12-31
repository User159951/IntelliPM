namespace IntelliPM.Application.Features.Releases.DTOs;

/// <summary>
/// Data Transfer Object for release statistics.
/// Contains aggregated statistics about releases for a project.
/// </summary>
public class ReleaseStatisticsDto
{
    /// <summary>
    /// Total number of releases in the project.
    /// </summary>
    public int TotalReleases { get; set; }

    /// <summary>
    /// Number of deployed releases.
    /// </summary>
    public int DeployedReleases { get; set; }

    /// <summary>
    /// Number of planned releases.
    /// </summary>
    public int PlannedReleases { get; set; }

    /// <summary>
    /// Number of failed releases.
    /// </summary>
    public int FailedReleases { get; set; }

    /// <summary>
    /// Average lead time in days (from creation to deployment).
    /// </summary>
    public double AverageLeadTime { get; set; }
}

