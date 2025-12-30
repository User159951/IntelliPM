using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Queries.Metrics;

public class GetMetricsSummaryQueryHandler : IRequestHandler<GetMetricsSummaryQuery, MetricsSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetMetricsSummaryQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<MetricsSummaryDto> Handle(GetMetricsSummaryQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = request.ProjectId.HasValue 
            ? $"dashboard-metrics:project:{request.ProjectId}" 
            : "dashboard-metrics:global";

        // Try get from cache
        var cached = await _cache.GetAsync<MetricsSummaryDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var tasksQuery = taskRepo.Query().AsNoTracking();
        
        if (request.ProjectId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.ProjectId == request.ProjectId.Value);

        var totalTasks = await tasksQuery.CountAsync(cancellationToken);
        var completedTasks = await tasksQuery.CountAsync(t => t.Status == TaskConstants.Statuses.Done, cancellationToken);
        var inProgressTasks = await tasksQuery.CountAsync(t => t.Status == TaskConstants.Statuses.InProgress, cancellationToken);
        var blockedTasks = await tasksQuery.CountAsync(t => t.Status == TaskConstants.Statuses.Blocked, cancellationToken);
        var todoTasks = await tasksQuery.CountAsync(t => t.Status == TaskConstants.Statuses.Todo, cancellationToken);

        var completionPercentage = totalTasks > 0 ? (completedTasks * 100.0 / totalTasks) : 0;

        // Calculate average completion time (tasks completed in last 30 days)
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
        var recentCompletedTasks = await tasksQuery
            .Where(t => t.Status == TaskConstants.Statuses.Done && t.UpdatedAt >= thirtyDaysAgo)
            .ToListAsync(cancellationToken);

        var avgCompletionTime = 0.0;
        if (recentCompletedTasks.Any())
        {
            avgCompletionTime = recentCompletedTasks
                .Average(t => (t.UpdatedAt - t.CreatedAt).TotalHours);
        }

        // Sprint metrics
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var sprintsQuery = sprintRepo.Query().AsNoTracking();
        if (request.ProjectId.HasValue)
            sprintsQuery = sprintsQuery.Where(s => s.ProjectId == request.ProjectId.Value);

        var totalSprints = await sprintsQuery.CountAsync(cancellationToken);
        var activeSprints = await sprintsQuery.CountAsync(s => s.Status == SprintConstants.Statuses.Active, cancellationToken);

        // Agent metrics
        var agentRepo = _unitOfWork.Repository<AgentExecutionLog>();
        var agentLogs = await agentRepo.Query().AsNoTracking().ToListAsync(cancellationToken);
        var totalAgentExecutions = agentLogs.Count;
        var successfulExecutions = agentLogs.Count(l => l.Status == "Success");
        var agentSuccessRate = totalAgentExecutions > 0 ? (successfulExecutions * 100.0 / totalAgentExecutions) : 0;
        var avgAgentResponseTime = agentLogs.Any() ? (int)agentLogs.Average(l => l.ExecutionTimeMs) : 0;

        var metrics = new MetricsSummaryDto
        {
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            InProgressTasks = inProgressTasks,
            BlockedTasks = blockedTasks,
            TodoTasks = todoTasks,
            CompletionPercentage = Math.Round(completionPercentage, 1),
            AverageCompletionTimeHours = Math.Round(avgCompletionTime, 1),
            TotalSprints = totalSprints,
            ActiveSprints = activeSprints,
            TotalAgentExecutions = totalAgentExecutions,
            AgentSuccessRate = Math.Round(agentSuccessRate, 1),
            AverageAgentResponseTimeMs = avgAgentResponseTime
        };

        // Cache for 10 minutes (metrics don't change often)
        await _cache.SetAsync(cacheKey, metrics, TimeSpan.FromMinutes(10), cancellationToken);

        return metrics;
    }
}

