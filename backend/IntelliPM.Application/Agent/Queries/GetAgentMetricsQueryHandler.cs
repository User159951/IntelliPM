using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Agent.Queries;

public class GetAgentMetricsQueryHandler : IRequestHandler<GetAgentMetricsQuery, AgentMetricsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAgentMetricsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AgentMetricsDto> Handle(GetAgentMetricsQuery request, CancellationToken cancellationToken)
    {
        var logsRepo = _unitOfWork.Repository<AgentExecutionLog>();
        var allLogs = await logsRepo.Query()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (!allLogs.Any())
        {
            return new AgentMetricsDto(
                TotalExecutions: 0,
                SuccessfulExecutions: 0,
                FailedExecutions: 0,
                SuccessRate: 0m,
                AverageExecutionTimeMs: 0,
                TotalCostUsd: 0m,
                LastExecutionAt: null,
                ByAgentType: new List<AgentTypeMetric>()
            );
        }

        // Overall metrics
        var totalExecutions = allLogs.Count;
        var successfulExecutions = allLogs.Count(log => log.Status == "Success");
        var failedExecutions = allLogs.Count(log => log.Status == "Error");
        var successRate = totalExecutions > 0 ? (successfulExecutions * 100m / totalExecutions) : 0m;
        var avgExecutionTime = (int)allLogs.Average(log => log.ExecutionTimeMs);
        var totalCost = allLogs.Sum(log => log.ExecutionCostUsd);
        var lastExecution = allLogs.Max(log => log.CreatedAt);

        // Metrics by agent type
        var byAgentType = allLogs
            .GroupBy(log => log.AgentId)
            .Select(g => new AgentTypeMetric(
                AgentId: g.Key,
                ExecutionCount: g.Count(),
                SuccessCount: g.Count(log => log.Status == "Success"),
                FailureCount: g.Count(log => log.Status == "Error"),
                SuccessRate: g.Count() > 0 ? (g.Count(log => log.Status == "Success") * 100m / g.Count()) : 0m,
                AvgExecutionTimeMs: (int)g.Average(log => log.ExecutionTimeMs)
            ))
            .OrderByDescending(m => m.ExecutionCount)
            .ToList();

        return new AgentMetricsDto(
            totalExecutions,
            successfulExecutions,
            failedExecutions,
            Math.Round(successRate, 2),
            avgExecutionTime,
            totalCost,
            lastExecution,
            byAgentType
        );
    }
}

