using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Projections.Queries;

/// <summary>
/// Handler for GetProjectOverviewQuery.
/// Retrieves project overview read model with aggregated metrics.
/// </summary>
public class GetProjectOverviewQueryHandler : IRequestHandler<GetProjectOverviewQuery, ProjectOverviewReadModelDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProjectOverviewQueryHandler> _logger;

    public GetProjectOverviewQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetProjectOverviewQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<ProjectOverviewReadModelDto?> Handle(GetProjectOverviewQuery request, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving project overview read model for project {ProjectId}", request.ProjectId);

        var readModel = await _unitOfWork.Repository<ProjectOverviewReadModel>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ProjectId == request.ProjectId, ct);

        if (readModel == null)
        {
            _logger.LogInformation("Project overview read model not found for project {ProjectId}", request.ProjectId);
            return null;
        }

        return new ProjectOverviewReadModelDto(
            readModel.ProjectId,
            readModel.ProjectName,
            readModel.ProjectType,
            readModel.Status,
            readModel.OwnerId,
            readModel.OwnerName,
            readModel.TotalMembers,
            readModel.ActiveMembers,
            readModel.GetTeamMembers(),
            readModel.TotalSprints,
            readModel.ActiveSprintsCount,
            readModel.CompletedSprintsCount,
            readModel.CurrentSprintId,
            readModel.CurrentSprintName,
            readModel.TotalTasks,
            readModel.CompletedTasks,
            readModel.InProgressTasks,
            readModel.TodoTasks,
            readModel.BlockedTasks,
            readModel.OverdueTasks,
            readModel.TotalStoryPoints,
            readModel.CompletedStoryPoints,
            readModel.RemainingStoryPoints,
            readModel.TotalDefects,
            readModel.OpenDefects,
            readModel.CriticalDefects,
            readModel.AverageVelocity,
            readModel.LastSprintVelocity,
            readModel.GetVelocityTrend(),
            readModel.ProjectHealth,
            readModel.HealthStatus,
            readModel.GetRiskFactors(),
            readModel.LastActivityAt,
            readModel.ActivitiesLast7Days,
            readModel.ActivitiesLast30Days,
            readModel.OverallProgress,
            readModel.SprintProgress,
            readModel.DaysUntilNextMilestone,
            readModel.LastUpdated,
            readModel.Version
        );
    }
}

