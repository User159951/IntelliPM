using MediatR;

namespace IntelliPM.Application.Agent.Queries;

public record GetAgentMetricsQuery : IRequest<AgentMetricsDto>;

public record AgentMetricsDto(
    int TotalExecutions,
    int SuccessfulExecutions,
    int FailedExecutions,
    decimal SuccessRate,
    int AverageExecutionTimeMs,
    decimal TotalCostUsd,
    DateTime? LastExecutionAt,
    List<AgentTypeMetric> ByAgentType
);

public record AgentTypeMetric(
    string AgentId,
    int ExecutionCount,
    int SuccessCount,
    int FailureCount,
    decimal SuccessRate,
    int AvgExecutionTimeMs
);

