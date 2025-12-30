using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to add multiple sprints to a release at once.
/// Performs batch operation for better performance.
/// </summary>
public record BulkAddSprintsToReleaseCommand : IRequest<int>
{
    /// <summary>
    /// ID of the release to add sprints to.
    /// </summary>
    public int ReleaseId { get; init; }

    /// <summary>
    /// List of sprint IDs to add to the release.
    /// </summary>
    public List<int> SprintIds { get; init; } = new();
}

