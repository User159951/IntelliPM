using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to update changelog for a release.
/// Can either auto-generate changelog or use manually provided changelog.
/// </summary>
public record UpdateChangeLogCommand : IRequest<Unit>
{
    /// <summary>
    /// ID of the release to update changelog for.
    /// </summary>
    public int ReleaseId { get; init; }

    /// <summary>
    /// Manual changelog content (if provided).
    /// Used when AutoGenerate is false.
    /// </summary>
    public string? ChangeLog { get; init; }

    /// <summary>
    /// If true, auto-generate changelog.
    /// If false, use the provided ChangeLog content.
    /// </summary>
    public bool AutoGenerate { get; init; }
}
