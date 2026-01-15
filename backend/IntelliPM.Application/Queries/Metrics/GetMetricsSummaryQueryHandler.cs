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

        // Project metrics
        var projectRepo = _unitOfWork.Repository<Project>();
        var projectsQuery = projectRepo.Query().AsNoTracking();
        if (request.ProjectId.HasValue)
            projectsQuery = projectsQuery.Where(p => p.Id == request.ProjectId.Value);
        var totalProjects = await projectsQuery.CountAsync(cancellationToken);

        // Task metrics
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var tasksQuery = taskRepo.Query().AsNoTracking();
        
        if (request.ProjectId.HasValue)
            tasksQuery = tasksQuery.Where(t => t.ProjectId == request.ProjectId.Value);

        var totalTasks = await tasksQuery.CountAsync(cancellationToken);
        var completedTasks = await tasksQuery.CountAsync(t => t.Status == TaskConstants.Statuses.Done, cancellationToken);
        var inProgressTasks = await tasksQuery.CountAsync(t => t.Status == TaskConstants.Statuses.InProgress, cancellationToken);
        var blockedTasks = await tasksQuery.CountAsync(t => t.Status == TaskConstants.Statuses.Blocked, cancellationToken);
        var todoTasks = await tasksQuery.CountAsync(t => t.Status == TaskConstants.Statuses.Todo, cancellationToken);
        var openTasks = todoTasks + inProgressTasks + blockedTasks;

        var completionPercentage = totalTasks > 0 ? (completedTasks * 100.0 / totalTasks) : 0;

        // Calculate average completion time (tasks completed in last 30 days)
        var now = DateTimeOffset.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var sixtyDaysAgo = now.AddDays(-60);
        
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

        // Calculate velocity (average story points from last 5 completed sprints)
        var completedSprints = await sprintsQuery
            .Where(s => s.Status == SprintConstants.Statuses.Completed)
            .OrderByDescending(s => s.EndDate ?? s.CreatedAt)
            .Take(5)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        double velocity = 0;
        if (completedSprints.Any())
        {
            var completedTasksInSprints = await taskRepo.Query()
                .Where(t => completedSprints.Contains(t.SprintId ?? 0) && 
                           (t.Status == TaskConstants.Statuses.Done || t.Status == "Completed"))
                .GroupBy(t => t.SprintId)
                .Select(g => g.Sum(t => t.StoryPoints != null ? t.StoryPoints.Value : 0))
                .ToListAsync(cancellationToken);
            
            velocity = completedTasksInSprints.Any() 
                ? Math.Round(completedTasksInSprints.Average(), 1) 
                : 0;
        }

        // Defect metrics
        var defectRepo = _unitOfWork.Repository<Defect>();
        var defectsQuery = defectRepo.Query().AsNoTracking();
        if (request.ProjectId.HasValue)
            defectsQuery = defectsQuery.Where(d => d.ProjectId == request.ProjectId.Value);

        var totalDefects = await defectsQuery.CountAsync(cancellationToken);
        var openDefects = await defectsQuery
            .CountAsync(d => d.Status == "Open" || d.Status == "InProgress" || d.Status == "Reopened", cancellationToken);

        // Agent metrics
        var agentRepo = _unitOfWork.Repository<AgentExecutionLog>();
        var agentLogs = await agentRepo.Query().AsNoTracking().ToListAsync(cancellationToken);
        var totalAgentExecutions = agentLogs.Count;
        var successfulExecutions = agentLogs.Count(l => l.Status == "Success");
        var agentSuccessRate = totalAgentExecutions > 0 ? (successfulExecutions * 100.0 / totalAgentExecutions) : 0;
        var avgAgentResponseTime = agentLogs.Any() ? (int)agentLogs.Average(l => l.ExecutionTimeMs) : 0;

        // Calculate trends (comparing last 30 days to previous 30 days)
        var trends = await CalculateTrends(
            tasksQuery, defectsQuery, projectsQuery, sprintsQuery, 
            thirtyDaysAgo, sixtyDaysAgo, cancellationToken);

        var metrics = new MetricsSummaryDto
        {
            TotalProjects = totalProjects,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            InProgressTasks = inProgressTasks,
            BlockedTasks = blockedTasks,
            TodoTasks = todoTasks,
            OpenTasks = openTasks,
            CompletionPercentage = Math.Round(completionPercentage, 1),
            AverageCompletionTimeHours = Math.Round(avgCompletionTime, 1),
            TotalSprints = totalSprints,
            ActiveSprints = activeSprints,
            Velocity = velocity,
            DefectsCount = openDefects,
            TotalDefects = totalDefects,
            TotalAgentExecutions = totalAgentExecutions,
            AgentSuccessRate = Math.Round(agentSuccessRate, 1),
            AverageAgentResponseTimeMs = avgAgentResponseTime,
            Trends = trends
        };

        // Cache for 5 minutes (metrics with trends need more frequent updates)
        await _cache.SetAsync(cacheKey, metrics, TimeSpan.FromMinutes(5), cancellationToken);

        return metrics;
    }

    private async Task<TrendData> CalculateTrends(
        IQueryable<ProjectTask> tasksQuery,
        IQueryable<Defect> defectsQuery,
        IQueryable<Project> projectsQuery,
        IQueryable<Sprint> sprintsQuery,
        DateTimeOffset thirtyDaysAgo,
        DateTimeOffset sixtyDaysAgo,
        CancellationToken cancellationToken)
    {
        // Current period (last 30 days) vs Previous period (30-60 days ago)
        
        // Open tasks trend
        var currentOpenTasks = await tasksQuery
            .CountAsync(t => t.CreatedAt >= thirtyDaysAgo && 
                           t.Status != TaskConstants.Statuses.Done, cancellationToken);
        var previousOpenTasks = await tasksQuery
            .CountAsync(t => t.CreatedAt >= sixtyDaysAgo && t.CreatedAt < thirtyDaysAgo && 
                           t.Status != TaskConstants.Statuses.Done, cancellationToken);
        
        // Blocked tasks trend
        var currentBlockedTasks = await tasksQuery
            .CountAsync(t => t.CreatedAt >= thirtyDaysAgo && 
                           t.Status == TaskConstants.Statuses.Blocked, cancellationToken);
        var previousBlockedTasks = await tasksQuery
            .CountAsync(t => t.CreatedAt >= sixtyDaysAgo && t.CreatedAt < thirtyDaysAgo && 
                           t.Status == TaskConstants.Statuses.Blocked, cancellationToken);
        
        // Defects trend
        var currentDefects = await defectsQuery
            .CountAsync(d => d.ReportedAt >= thirtyDaysAgo, cancellationToken);
        var previousDefects = await defectsQuery
            .CountAsync(d => d.ReportedAt >= sixtyDaysAgo && d.ReportedAt < thirtyDaysAgo, cancellationToken);

        // Projects trend
        var currentProjects = await projectsQuery
            .CountAsync(p => p.CreatedAt >= thirtyDaysAgo, cancellationToken);
        var previousProjects = await projectsQuery
            .CountAsync(p => p.CreatedAt >= sixtyDaysAgo && p.CreatedAt < thirtyDaysAgo, cancellationToken);

        // Sprints trend
        var currentSprints = await sprintsQuery
            .CountAsync(s => s.CreatedAt >= thirtyDaysAgo, cancellationToken);
        var previousSprints = await sprintsQuery
            .CountAsync(s => s.CreatedAt >= sixtyDaysAgo && s.CreatedAt < thirtyDaysAgo, cancellationToken);

        return new TrendData
        {
            ProjectsTrend = CalculatePercentageChange(previousProjects, currentProjects),
            SprintsTrend = CalculatePercentageChange(previousSprints, currentSprints),
            OpenTasksTrend = CalculatePercentageChange(previousOpenTasks, currentOpenTasks),
            BlockedTasksTrend = CalculatePercentageChange(previousBlockedTasks, currentBlockedTasks),
            DefectsTrend = CalculatePercentageChange(previousDefects, currentDefects),
            VelocityTrend = 0 // Velocity trend calculated separately based on sprint completions
        };
    }

    private static double CalculatePercentageChange(int previous, int current)
    {
        if (previous == 0)
            return current > 0 ? 100 : 0;
        
        return Math.Round(((current - previous) * 100.0 / previous), 1);
    }
}

