using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Enums;
using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to create a new release for a project.
/// </summary>
public record CreateReleaseCommand : IRequest<ReleaseDto>
{
    /// <summary>
    /// ID of the project this release belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// Name of the release (e.g., "v2.1.0", "Summer Release 2024").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Semantic version of the release (e.g., "2.1.0", "1.0.0-beta.1").
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Optional description providing additional details about the release.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Planned date for the release.
    /// </summary>
    public DateTimeOffset PlannedDate { get; init; }

    /// <summary>
    /// Type of release (Major, Minor, Patch, Hotfix).
    /// </summary>
    public ReleaseType Type { get; init; }

    /// <summary>
    /// Indicates if this is a pre-release (beta, alpha, or release candidate).
    /// </summary>
    public bool IsPreRelease { get; init; }

    /// <summary>
    /// Git tag name associated with this release (e.g., "v2.1.0").
    /// </summary>
    public string? TagName { get; init; }

    /// <summary>
    /// List of sprint IDs to add to the release.
    /// </summary>
    public List<int> SprintIds { get; init; } = new();
}
