using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to generate changelog for a release.
/// Returns the generated changelog in markdown format.
/// </summary>
public record GenerateChangeLogCommand : IRequest<string>
{
    /// <summary>
    /// ID of the release to generate changelog for.
    /// </summary>
    public int ReleaseId { get; init; }
}
