using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Queries.Metrics;

public class GetSprintVelocityChartQueryHandler : IRequestHandler<GetSprintVelocityChartQuery, SprintVelocityChartResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetSprintVelocityChartQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<SprintVelocityChartResponse> Handle(GetSprintVelocityChartQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = request.ProjectId.HasValue 
            ? $"sprint-velocity:project:{request.ProjectId}" 
            : "sprint-velocity:global";

        // Try get from cache
        var cached = await _cache.GetAsync<SprintVelocityChartResponse>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var taskRepo = _unitOfWork.Repository<ProjectTask>();

        // Get last 6 completed sprints
        var completedSprints = await sprintRepo.Query()
            .Where(s => s.Status == "Completed" && 
                       (!request.ProjectId.HasValue || s.ProjectId == request.ProjectId.Value))
            .OrderByDescending(s => s.EndDate ?? s.CreatedAt)
            .Take(6)
            .Select(s => new { s.Id, s.Number, s.EndDate, s.CreatedAt })
            .ToListAsync(cancellationToken);

        if (!completedSprints.Any())
        {
            return new SprintVelocityChartResponse();
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

        var result = new SprintVelocityChartResponse();

        foreach (var sprint in completedSprints.OrderBy(s => s.EndDate ?? s.CreatedAt))
        {
            result.Sprints.Add(new SprintVelocityData
            {
                Number = sprint.Number,
                StoryPoints = taskPointsBySprint.GetValueOrDefault(sprint.Id, 0),
                CompletedDate = sprint.EndDate ?? sprint.CreatedAt
            });
        }

        // Cache for 10 minutes (sprint velocity doesn't change often)
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);

        return result;
    }
}
