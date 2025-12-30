using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Projections.Queries;

/// <summary>
/// Handler for GetSprintSummaryQuery.
/// Retrieves sprint summary read model with pre-calculated metrics.
/// </summary>
public class GetSprintSummaryQueryHandler : IRequestHandler<GetSprintSummaryQuery, SprintSummaryReadModelDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSprintSummaryQueryHandler> _logger;

    public GetSprintSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSprintSummaryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<SprintSummaryReadModelDto?> Handle(GetSprintSummaryQuery request, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving sprint summary read model for sprint {SprintId}", request.SprintId);

        var readModel = await _unitOfWork.Repository<SprintSummaryReadModel>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SprintId == request.SprintId, ct);

        if (readModel == null)
        {
            _logger.LogInformation("Sprint summary read model not found for sprint {SprintId}", request.SprintId);
            return null;
        }

        return new SprintSummaryReadModelDto(
            readModel.SprintId,
            readModel.SprintName,
            readModel.Status,
            readModel.StartDate,
            readModel.EndDate,
            readModel.PlannedCapacity,
            readModel.TotalTasks,
            readModel.CompletedTasks,
            readModel.InProgressTasks,
            readModel.TodoTasks,
            readModel.TotalStoryPoints,
            readModel.CompletedStoryPoints,
            readModel.InProgressStoryPoints,
            readModel.RemainingStoryPoints,
            readModel.CompletionPercentage,
            readModel.VelocityPercentage,
            readModel.CapacityUtilization,
            readModel.EstimatedDaysRemaining,
            readModel.IsOnTrack,
            readModel.AverageVelocity,
            readModel.GetBurndownData(),
            readModel.LastUpdated,
            readModel.Version
        );
    }
}

