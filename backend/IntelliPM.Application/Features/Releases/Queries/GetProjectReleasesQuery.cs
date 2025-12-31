using MediatR;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Query to retrieve all releases for a project with optional status filtering.
/// </summary>
public record GetProjectReleasesQuery(
    int ProjectId,
    ReleaseStatus? Status = null
) : IRequest<List<ReleaseDto>>;

