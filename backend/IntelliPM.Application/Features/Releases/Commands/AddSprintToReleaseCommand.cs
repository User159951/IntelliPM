using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to add a sprint to a release.
/// Links a sprint to a release by setting the sprint's ReleaseId property.
/// </summary>
public record AddSprintToReleaseCommand : IRequest<Unit>
{
    /// <summary>
    /// ID of the release to add the sprint to.
    /// </summary>
    public int ReleaseId { get; init; }

    /// <summary>
    /// ID of the sprint to add to the release.
    /// </summary>
    public int SprintId { get; init; }
}

