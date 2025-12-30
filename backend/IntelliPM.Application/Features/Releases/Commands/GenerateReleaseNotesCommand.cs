using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to generate release notes for a release.
/// Returns the generated release notes in markdown format.
/// </summary>
public record GenerateReleaseNotesCommand : IRequest<string>
{
    /// <summary>
    /// ID of the release to generate notes for.
    /// </summary>
    public int ReleaseId { get; init; }
}
