using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Projections.Queries;

/// <summary>
/// Handler for GetTaskBoardQuery.
/// Retrieves task board read model optimized for Kanban board rendering.
/// </summary>
public class GetTaskBoardQueryHandler : IRequestHandler<GetTaskBoardQuery, TaskBoardReadModelDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetTaskBoardQueryHandler> _logger;

    public GetTaskBoardQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetTaskBoardQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<TaskBoardReadModelDto?> Handle(GetTaskBoardQuery request, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving task board read model for project {ProjectId}", request.ProjectId);

        var readModel = await _unitOfWork.Repository<TaskBoardReadModel>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ProjectId == request.ProjectId, ct);

        if (readModel == null)
        {
            _logger.LogInformation("Task board read model not found for project {ProjectId}", request.ProjectId);
            return null;
        }

        return new TaskBoardReadModelDto(
            readModel.ProjectId,
            readModel.TodoCount,
            readModel.InProgressCount,
            readModel.DoneCount,
            readModel.TotalTaskCount,
            readModel.TodoStoryPoints,
            readModel.InProgressStoryPoints,
            readModel.DoneStoryPoints,
            readModel.TotalStoryPoints,
            readModel.GetTodoTasks(),
            readModel.GetInProgressTasks(),
            readModel.GetDoneTasks(),
            readModel.LastUpdated,
            readModel.Version
        );
    }
}

