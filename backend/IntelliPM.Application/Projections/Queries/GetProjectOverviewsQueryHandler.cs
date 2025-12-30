using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Projections.Queries;

/// <summary>
/// Handler for GetProjectOverviewsQuery.
/// Retrieves paginated list of project overview read models with filtering.
/// </summary>
public class GetProjectOverviewsQueryHandler : IRequestHandler<GetProjectOverviewsQuery, PagedResponse<ProjectOverviewReadModelDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetProjectOverviewsQueryHandler> _logger;

    public GetProjectOverviewsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetProjectOverviewsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<PagedResponse<ProjectOverviewReadModelDto>> Handle(GetProjectOverviewsQuery request, CancellationToken ct)
    {
        _logger.LogDebug(
            "Retrieving project overviews: OrganizationId={OrganizationId}, Status={Status}, Page={Page}, PageSize={PageSize}",
            request.OrganizationId,
            request.Status,
            request.Page,
            request.PageSize);

        var query = _unitOfWork.Repository<ProjectOverviewReadModel>()
            .Query()
            .AsNoTracking();

        // Apply organization filter (multi-tenancy)
        var userOrganizationId = _currentUserService.GetOrganizationId();
        if (!_currentUserService.IsAdmin())
        {
            // Non-admin users can only see their organization's projects
            query = query.Where(p => p.OrganizationId == userOrganizationId);
        }
        else if (request.OrganizationId.HasValue)
        {
            // Admins can filter by organization if specified
            query = query.Where(p => p.OrganizationId == request.OrganizationId.Value);
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(p => p.Status == request.Status);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var readModels = await query
            .OrderByDescending(p => p.LastUpdated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = readModels.Select(rm => new ProjectOverviewReadModelDto(
            rm.ProjectId,
            rm.ProjectName,
            rm.ProjectType,
            rm.Status,
            rm.OwnerId,
            rm.OwnerName,
            rm.TotalMembers,
            rm.ActiveMembers,
            rm.GetTeamMembers(),
            rm.TotalSprints,
            rm.ActiveSprintsCount,
            rm.CompletedSprintsCount,
            rm.CurrentSprintId,
            rm.CurrentSprintName,
            rm.TotalTasks,
            rm.CompletedTasks,
            rm.InProgressTasks,
            rm.TodoTasks,
            rm.BlockedTasks,
            rm.OverdueTasks,
            rm.TotalStoryPoints,
            rm.CompletedStoryPoints,
            rm.RemainingStoryPoints,
            rm.TotalDefects,
            rm.OpenDefects,
            rm.CriticalDefects,
            rm.AverageVelocity,
            rm.LastSprintVelocity,
            rm.GetVelocityTrend(),
            rm.ProjectHealth,
            rm.HealthStatus,
            rm.GetRiskFactors(),
            rm.LastActivityAt,
            rm.ActivitiesLast7Days,
            rm.ActivitiesLast30Days,
            rm.OverallProgress,
            rm.SprintProgress,
            rm.DaysUntilNextMilestone,
            rm.LastUpdated,
            rm.Version
        )).ToList();

        return new PagedResponse<ProjectOverviewReadModelDto>(
            dtos,
            page,
            pageSize,
            totalCount
        );
    }
}

