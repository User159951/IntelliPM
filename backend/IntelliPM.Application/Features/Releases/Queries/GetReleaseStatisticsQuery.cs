using MediatR;
using IntelliPM.Application.Features.Releases.DTOs;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Query to retrieve release statistics for a project.
/// Returns aggregated statistics about releases.
/// </summary>
public record GetReleaseStatisticsQuery(int ProjectId) : IRequest<ReleaseStatisticsDto>;

