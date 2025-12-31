using IntelliPM.Application.Features.Releases.DTOs;
using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to deploy a release.
/// </summary>
public record DeployReleaseCommand(int Id) : IRequest<ReleaseDto>;

