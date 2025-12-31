using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Handler for GetProjectReleasesQuery.
/// Retrieves all releases for a project with optional status filtering.
/// </summary>
public class GetProjectReleasesQueryHandler : IRequestHandler<GetProjectReleasesQuery, List<ReleaseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetProjectReleasesQueryHandler> _logger;

    public GetProjectReleasesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetProjectReleasesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ReleaseDto>> Handle(GetProjectReleasesQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            return new List<ReleaseDto>();
        }

        // Verify project exists and belongs to organization
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");
        }

        // Build query
        var query = _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.CreatedBy)
            .Include(r => r.ReleasedBy)
            .Include(r => r.Sprints)
            .Include(r => r.QualityGates)
            .Where(r => r.ProjectId == request.ProjectId && r.OrganizationId == organizationId);

        // Apply status filter if provided
        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        var releases = await query
            .OrderByDescending(r => r.PlannedDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var releaseDtos = new List<ReleaseDto>();

        foreach (var release in releases)
        {
            // Calculate task counts from sprints
            var totalTasks = 0;
            var completedTasks = 0;

            foreach (var sprint in release.Sprints)
            {
                // Note: This assumes Sprint has navigation to Tasks via SprintItems
                // Adjust based on actual entity structure
                // For now, we'll use SprintCount as a placeholder
            }

            var overallQualityStatus = release.GetOverallQualityStatus();

            releaseDtos.Add(new ReleaseDto
            {
                Id = release.Id,
                ProjectId = release.ProjectId,
                Name = release.Name,
                Version = release.Version,
                Description = release.Description,
                Type = release.Type.ToString(),
                Status = release.Status.ToString(),
                PlannedDate = release.PlannedDate,
                ActualReleaseDate = release.ActualReleaseDate,
                ReleaseNotes = release.ReleaseNotes,
                ChangeLog = release.ChangeLog,
                IsPreRelease = release.IsPreRelease,
                TagName = release.TagName,
                SprintCount = release.Sprints.Count,
                CompletedTasksCount = completedTasks,
                TotalTasksCount = totalTasks,
                OverallQualityStatus = overallQualityStatus.ToString(),
                CreatedAt = release.CreatedAt,
                CreatedByName = release.CreatedBy?.Username ?? "Unknown",
                ReleasedByName = release.ReleasedBy?.Username
            });
        }

        return releaseDtos;
    }
}
