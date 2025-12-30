using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Queries.Metrics;

public class GetSprintBurndownQueryHandler : IRequestHandler<GetSprintBurndownQuery, SprintBurndownResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSprintBurndownQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SprintBurndownResponse> Handle(GetSprintBurndownQuery request, CancellationToken cancellationToken)
    {
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var taskRepo = _unitOfWork.Repository<ProjectTask>();

        var sprint = await sprintRepo.GetByIdAsync(request.SprintId, cancellationToken);
        if (sprint == null)
        {
            throw new InvalidOperationException($"Sprint with ID {request.SprintId} not found");
        }

        if (!sprint.StartDate.HasValue || !sprint.EndDate.HasValue)
        {
            return new SprintBurndownResponse { Days = new List<BurndownDayData>() };
        }

        var sprintStart = sprint.StartDate.Value;
        var sprintEnd = sprint.EndDate.Value;
        var sprintDuration = (sprintEnd - sprintStart).Days;
        
        if (sprintDuration <= 0)
        {
            return new SprintBurndownResponse { Days = new List<BurndownDayData>() };
        }

        // Get all tasks in the sprint
        var sprintTasks = await taskRepo.Query()
            .Where(t => t.SprintId == request.SprintId)
            .ToListAsync(cancellationToken);

        var totalStoryPoints = sprintTasks.Sum(t => t.StoryPoints?.Value ?? 0);
        var idealBurnRate = (double)totalStoryPoints / sprintDuration;

        var result = new SprintBurndownResponse();
        var today = DateTimeOffset.UtcNow;

        for (int day = 1; day <= sprintDuration; day++)
        {
            var currentDate = sprintStart.AddDays(day - 1);
            var ideal = (int)Math.Max(0, totalStoryPoints - (idealBurnRate * day));

            // Calculate actual remaining story points at this date
            var completedTasksAtDate = sprintTasks
                .Where(t => (t.Status == "Done" || t.Status == "Completed") &&
                           (t.UpdatedAt <= currentDate || (t.UpdatedAt == default && t.CreatedAt <= currentDate)))
                .Sum(t => t.StoryPoints?.Value ?? 0);

            var actual = totalStoryPoints - completedTasksAtDate;

            result.Days.Add(new BurndownDayData
            {
                Day = day,
                Ideal = ideal,
                Actual = Math.Max(0, actual)
            });
        }

        return result;
    }
}
