using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to update release notes for a release.
/// Can either auto-generate notes or use manually provided notes.
/// </summary>
public record UpdateReleaseNotesCommand : IRequest<Unit>
{
    /// <summary>
    /// ID of the release to update notes for.
    /// </summary>
    public int ReleaseId { get; init; }

    /// <summary>
    /// Manual release notes content (if provided).
    /// Used when AutoGenerate is false.
    /// </summary>
    public string? ReleaseNotes { get; init; }

    /// <summary>
    /// If true, auto-generate release notes.
    /// If false, use the provided ReleaseNotes content.
    /// </summary>
    public bool AutoGenerate { get; init; }
}
