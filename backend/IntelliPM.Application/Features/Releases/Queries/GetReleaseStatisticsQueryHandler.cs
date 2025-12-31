using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Handler for GetReleaseStatisticsQuery.
/// Calculates and returns aggregated statistics about releases for a project.
/// </summary>
public class GetReleaseStatisticsQueryHandler : IRequestHandler<GetReleaseStatisticsQuery, ReleaseStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetReleaseStatisticsQueryHandler> _logger;

    public GetReleaseStatisticsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetReleaseStatisticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReleaseStatisticsDto> Handle(GetReleaseStatisticsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            return new ReleaseStatisticsDto();
        }

        // Verify project exists
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId, cancellationToken);

        if (project == null)
        {
            return new ReleaseStatisticsDto();
        }

        var releases = await _unitOfWork.Repository<Release>()
            .Query()
            .Where(r => r.ProjectId == request.ProjectId && r.OrganizationId == organizationId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalReleases = releases.Count;
        var deployedReleases = releases.Count(r => r.Status == ReleaseStatus.Deployed);
        var plannedReleases = releases.Count(r => r.Status == ReleaseStatus.Planned);
        var failedReleases = releases.Count(r => r.Status == ReleaseStatus.Failed);

        // Calculate average lead time (from creation to deployment) for deployed releases
        var deployedReleasesWithDates = releases
            .Where(r => r.Status == ReleaseStatus.Deployed && r.ActualReleaseDate.HasValue)
            .ToList();

        var averageLeadTime = 0.0;
        if (deployedReleasesWithDates.Any())
        {
            var leadTimes = deployedReleasesWithDates
                .Select(r => (r.ActualReleaseDate!.Value - r.CreatedAt).TotalDays)
                .ToList();
            averageLeadTime = leadTimes.Average();
        }

        return new ReleaseStatisticsDto
        {
            TotalReleases = totalReleases,
            DeployedReleases = deployedReleases,
            PlannedReleases = plannedReleases,
            FailedReleases = failedReleases,
            AverageLeadTime = Math.Round(averageLeadTime, 2)
        };
    }
}
