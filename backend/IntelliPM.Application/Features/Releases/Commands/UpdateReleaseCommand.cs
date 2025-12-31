using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Enums;
using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to update an existing release.
/// </summary>
public record UpdateReleaseCommand : IRequest<ReleaseDto>
{
    /// <summary>
    /// ID of the release to update.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Updated name of the release.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Updated semantic version of the release.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Updated description of the release.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Updated planned date for the release.
    /// </summary>
    public DateTimeOffset PlannedDate { get; init; }

    /// <summary>
    /// Updated status of the release.
    /// </summary>
    public ReleaseStatus Status { get; init; }
}

