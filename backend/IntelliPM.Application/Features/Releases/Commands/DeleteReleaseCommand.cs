using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to delete a release.
/// </summary>
public record DeleteReleaseCommand(int Id) : IRequest<Unit>;

