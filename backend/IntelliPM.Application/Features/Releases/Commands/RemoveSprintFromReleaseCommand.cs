using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to remove a sprint from its release.
/// Unlinks a sprint from a release by setting the sprint's ReleaseId to null.
/// </summary>
public record RemoveSprintFromReleaseCommand : IRequest<Unit>
{
    /// <summary>
    /// ID of the sprint to remove from its release.
    /// </summary>
    public int SprintId { get; init; }
}

