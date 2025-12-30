using MediatR;

namespace IntelliPM.Application.Agent.Queries;

public record GetAgentAuditLogsQuery : IRequest<GetAgentAuditLogsResponse>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? AgentId { get; init; }
    public string? UserId { get; init; }
    public string? Status { get; init; }
}

public record GetAgentAuditLogsResponse(
    List<AgentExecutionLogDto> Logs,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record AgentExecutionLogDto(
    Guid Id,
    string AgentId,
    string UserId,
    string UserInput,
    string? AgentResponse,
    string? ToolsCalled,
    string Status,
    int ExecutionTimeMs,
    decimal ExecutionCostUsd,
    DateTime CreatedAt,
    string? ErrorMessage
);

