using MediatR;
using IntelliPM.Application.Features.Releases.DTOs;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Query to retrieve a release by its ID.
/// </summary>
public record GetReleaseByIdQuery(int Id) : IRequest<ReleaseDto?>;

