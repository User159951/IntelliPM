using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Release entity representing a software release or version deployment.
/// Releases track the lifecycle of software versions from planning to deployment.
/// </summary>
public class Release : IAggregateRoot
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
    /// Maximum length: 200 characters.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Semantic version of the release (e.g., "2.1.0", "1.0.0-beta.1").
    /// Must follow semantic versioning format.
    /// Maximum length: 50 characters.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Optional description providing additional details about the release.
    /// Maximum length: 2000 characters.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of release (Major, Minor, Patch, Hotfix).
    /// </summary>
    public ReleaseType Type { get; set; } = ReleaseType.Minor;

    /// <summary>
    /// Current status of the release (Planned, InProgress, Testing, ReadyForDeployment, Deployed, Failed, Cancelled).
    /// Default: Planned.
    /// </summary>
    public ReleaseStatus Status { get; set; } = ReleaseStatus.Planned;

    /// <summary>
    /// Planned date for the release. This is the target date when the release should be deployed.
    /// Required field.
    /// </summary>
    public DateTimeOffset PlannedDate { get; set; }

    /// <summary>
    /// Actual date and time when the release was deployed.
    /// Null if the release has not been deployed yet.
    /// </summary>
    public DateTimeOffset? ActualReleaseDate { get; set; }

    /// <summary>
    /// Release notes in markdown format describing what's new in this release.
    /// Maximum length: 5000 characters.
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Change log in markdown format listing all changes, fixes, and features.
    /// Can be auto-generated or manually entered.
    /// Maximum length: 5000 characters.
    /// </summary>
    public string? ChangeLog { get; set; }

    /// <summary>
    /// Indicates if this is a pre-release (beta, alpha, or release candidate).
    /// Default: false.
    /// </summary>
    public bool IsPreRelease { get; set; } = false;

    /// <summary>
    /// Git tag name associated with this release (e.g., "v2.1.0", "release-2.1.0").
    /// Maximum length: 100 characters.
    /// </summary>
    public string? TagName { get; set; }

    /// <summary>
    /// Organization ID for multi-tenancy isolation.
    /// </summary>
    public int OrganizationId { get; set; }

    /// <summary>
    /// Date and time when the release was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Date and time when the release was last updated.
    /// Null if the release has never been updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// ID of the user who created the release.
    /// </summary>
    public int CreatedById { get; set; }

    /// <summary>
    /// ID of the user who deployed the release.
    /// Null if the release has not been deployed yet.
    /// </summary>
    public int? ReleasedById { get; set; }

    // Navigation properties

    /// <summary>
    /// Project this release belongs to.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Collection of sprints included in this release.
    /// </summary>
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();

    /// <summary>
    /// Collection of quality gates for this release.
    /// </summary>
    public ICollection<QualityGate> QualityGates { get; set; } = new List<QualityGate>();

    /// <summary>
    /// User who created the release.
    /// </summary>
    public User CreatedBy { get; set; } = null!;

    /// <summary>
    /// User who deployed the release.
    /// Null if the release has not been deployed yet.
    /// </summary>
    public User? ReleasedBy { get; set; }

    // Domain methods

    /// <summary>
    /// Validates if a sprint can be added to this release.
    /// </summary>
    /// <param name="sprint">The sprint to validate.</param>
    /// <returns>True if the sprint can be added, false otherwise.</returns>
    public bool CanAddSprint(Sprint sprint)
    {
        if (sprint.ProjectId != ProjectId)
            return false;

        if (Status == ReleaseStatus.Deployed || Status == ReleaseStatus.Cancelled)
            return false;

        // Sprint should ideally be completed or in progress
        // Optionally: check if sprint end date is before or close to release planned date

        return true;
    }

    /// <summary>
    /// Gets the total number of sprints in this release.
    /// </summary>
    /// <returns>The count of sprints in this release.</returns>
    public int GetSprintCount()
    {
        return Sprints?.Count ?? 0;
    }

    /// <summary>
    /// Checks if release can be deployed based on status.
    /// </summary>
    /// <returns>True if release can be deployed, false otherwise.</returns>
    public bool CanDeploy()
    {
        return Status == ReleaseStatus.ReadyForDeployment;
    }

    /// <summary>
    /// Marks release as deployed.
    /// </summary>
    /// <param name="releasedByUserId">ID of the user who is deploying the release.</param>
    /// <exception cref="InvalidOperationException">Thrown if release is not in ReadyForDeployment status.</exception>
    public void MarkAsDeployed(int releasedByUserId)
    {
        if (Status != ReleaseStatus.ReadyForDeployment)
            throw new InvalidOperationException("Release must be in ReadyForDeployment status to be deployed");

        Status = ReleaseStatus.Deployed;
        ActualReleaseDate = DateTimeOffset.UtcNow;
        ReleasedById = releasedByUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Checks if all required quality gates have passed.
    /// </summary>
    /// <returns>True if all required quality gates have passed or been skipped, false otherwise.</returns>
    public bool AreQualityGatesPassed()
    {
        if (!QualityGates.Any())
            return true; // No gates defined = no restrictions

        return QualityGates
            .Where(qg => qg.IsRequired)
            .All(qg => qg.Status == QualityGateStatus.Passed || qg.Status == QualityGateStatus.Skipped);
    }

    /// <summary>
    /// Gets the overall quality gate status for the release.
    /// </summary>
    /// <returns>The overall quality gate status.</returns>
    public QualityGateStatus GetOverallQualityStatus()
    {
        if (!QualityGates.Any())
            return QualityGateStatus.Pending;

        // Any required gate failed = overall failed
        if (QualityGates.Any(qg => qg.IsRequired && qg.Status == QualityGateStatus.Failed))
            return QualityGateStatus.Failed;

        // Any gate has warning = overall warning
        if (QualityGates.Any(qg => qg.Status == QualityGateStatus.Warning))
            return QualityGateStatus.Warning;

        // Any gate pending = overall pending
        if (QualityGates.Any(qg => qg.Status == QualityGateStatus.Pending))
            return QualityGateStatus.Pending;

        // All passed or skipped = overall passed
        if (QualityGates.All(qg => qg.Status == QualityGateStatus.Passed || qg.Status == QualityGateStatus.Skipped))
            return QualityGateStatus.Passed;

        return QualityGateStatus.Pending;
    }

    /// <summary>
    /// Validates if release can be deployed based on quality gates.
    /// </summary>
    /// <returns>True if release can be deployed (status is ReadyForDeployment and all quality gates passed), false otherwise.</returns>
    public bool CanDeployWithQualityGates()
    {
        return Status == ReleaseStatus.ReadyForDeployment && AreQualityGatesPassed();
    }
}

