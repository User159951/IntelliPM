using IntelliPM.Application.Sprints.Queries;
using MediatR;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Query to get sprints that can be added to a release.
/// Returns sprints that belong to the project, are not already in another release,
/// and have a status of Completed or InProgress.
/// </summary>
public record GetAvailableSprintsForReleaseQuery : IRequest<List<SprintDto>>
{
    /// <summary>
    /// ID of the project to get sprints for.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// Optional ID of the release being edited.
    /// If provided, sprints already in this release will be included.
    /// If null, only sprints not in any release will be returned.
    /// </summary>
    public int? ReleaseId { get; init; }
}

