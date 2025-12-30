namespace IntelliPM.Application.Features.Releases.DTOs;

/// <summary>
/// Data Transfer Object for Release entity.
/// Contains release information with calculated properties and sprint details.
/// </summary>
public class ReleaseDto
{
    /// <summary>
    /// Unique identifier for the release.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the project this release belongs to.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Name of the release (e.g., "v2.1.0", "Summer Release 2024").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Semantic version of the release (e.g., "2.1.0", "1.0.0-beta.1").
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Description of the release.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of release as string: "Major", "Minor", "Patch", "Hotfix".
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Status of release as string: "Planned", "InProgress", "Testing", "ReadyForDeployment", "Deployed", "Failed", "Cancelled".
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Planned date for the release.
    /// </summary>
    public DateTimeOffset PlannedDate { get; set; }

    /// <summary>
    /// Actual date and time when the release was deployed.
    /// Null if not deployed yet.
    /// </summary>
    public DateTimeOffset? ActualReleaseDate { get; set; }

    /// <summary>
    /// Release notes in markdown format.
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Change log in markdown format.
    /// </summary>
    public string? ChangeLog { get; set; }

    /// <summary>
    /// Indicates if this is a pre-release (beta, alpha, or release candidate).
    /// </summary>
    public bool IsPreRelease { get; set; }

    /// <summary>
    /// Git tag name associated with this release.
    /// </summary>
    public string? TagName { get; set; }

    /// <summary>
    /// Count of sprints included in this release.
    /// </summary>
    public int SprintCount { get; set; }

    /// <summary>
    /// Total number of completed tasks across all sprints in this release.
    /// </summary>
    public int CompletedTasksCount { get; set; }

    /// <summary>
    /// Total number of tasks across all sprints in this release.
    /// </summary>
    public int TotalTasksCount { get; set; }

    /// <summary>
    /// Overall quality status of the release (e.g., "Passed", "Failed", "Pending").
    /// Calculated from quality gates.
    /// </summary>
    public string? OverallQualityStatus { get; set; }

    /// <summary>
    /// List of quality gates for this release.
    /// Optional, populated when needed (e.g., when fetching release details with quality gates).
    /// </summary>
    public List<QualityGateDto>? QualityGates { get; set; }

    /// <summary>
    /// Date and time when the release was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Name of the user who created the release.
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the user who deployed the release.
    /// Null if not deployed yet.
    /// </summary>
    public string? ReleasedByName { get; set; }

    /// <summary>
    /// List of sprints included in this release.
    /// Optional, populated when needed (e.g., when fetching release details).
    /// </summary>
    public List<ReleaseSprintDto>? Sprints { get; set; }
}

