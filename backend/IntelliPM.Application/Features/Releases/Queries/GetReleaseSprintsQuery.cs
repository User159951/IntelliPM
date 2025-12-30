using IntelliPM.Application.Features.Releases.DTOs;
using MediatR;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Query to get all sprints linked to a release.
/// Returns sprints with task completion metrics.
/// </summary>
public record GetReleaseSprintsQuery : IRequest<List<ReleaseSprintDto>>
{
    /// <summary>
    /// ID of the release to get sprints for.
    /// </summary>
    public int ReleaseId { get; init; }
}

