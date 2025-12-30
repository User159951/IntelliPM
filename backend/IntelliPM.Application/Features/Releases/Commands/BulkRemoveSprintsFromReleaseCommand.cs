using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to remove multiple sprints from their releases at once.
/// Performs batch operation for better performance.
/// </summary>
public record BulkRemoveSprintsFromReleaseCommand : IRequest<int>
{
    /// <summary>
    /// List of sprint IDs to remove from their releases.
    /// </summary>
    public List<int> SprintIds { get; init; } = new();
}

