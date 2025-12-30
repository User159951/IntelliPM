using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Queries.Metrics;

public class GetTeamVelocityQueryHandler : IRequestHandler<GetTeamVelocityQuery, TeamVelocityResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeamVelocityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TeamVelocityResponse> Handle(GetTeamVelocityQuery request, CancellationToken cancellationToken)
    {
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var taskRepo = _unitOfWork.Repository<ProjectTask>();

        // Get all completed sprints
        var completedSprints = await sprintRepo.Query()
            .Where(s => s.Status == "Completed" && 
                       (!request.ProjectId.HasValue || s.ProjectId == request.ProjectId.Value))
            .OrderBy(s => s.EndDate ?? s.CreatedAt)
            .Select(s => new { s.Id, s.Number, s.EndDate, s.CreatedAt })
            .ToListAsync(cancellationToken);

        if (!completedSprints.Any())
        {
            return new TeamVelocityResponse();
        }

        var sprintIds = completedSprints.Select(s => s.Id).ToList();

        // Get all completed tasks for these sprints in one query
        var completedTasks = await taskRepo.Query()
            .Where(t => sprintIds.Contains(t.SprintId ?? 0) && 
                       (t.Status == "Done" || t.Status == "Completed"))
            .GroupBy(t => t.SprintId)
            .Select(g => new { SprintId = g.Key, StoryPoints = g.Sum(t => t.StoryPoints != null ? t.StoryPoints.Value : 0) })
            .ToListAsync(cancellationToken);

        var taskPointsBySprint = completedTasks.ToDictionary(t => t.SprintId ?? 0, t => t.StoryPoints);

        var result = new TeamVelocityResponse();

        foreach (var sprint in completedSprints)
        {
            result.Velocity.Add(new TeamVelocityData
            {
                Date = sprint.EndDate ?? sprint.CreatedAt,
                StoryPoints = taskPointsBySprint.GetValueOrDefault(sprint.Id, 0),
                SprintNumber = sprint.Number
            });
        }

        return result;
    }
}
