using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get AI decision logs for an organization with filtering and pagination.
/// </summary>
public record GetAIDecisionLogsQuery : IRequest<PagedResponse<AIDecisionLogDto>>
{
    public int OrganizationId { get; init; }
    public string? DecisionType { get; init; }
    public string? AgentType { get; init; }
    public string? EntityType { get; init; }
    public int? EntityId { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public bool? RequiresApproval { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// DTO for AI decision log summary.
/// </summary>
public record AIDecisionLogDto(
    Guid DecisionId,
    string DecisionType,
    string AgentType,
    string EntityType,
    int EntityId,
    string EntityName,
    string Question,
    string Decision,
    decimal ConfidenceScore,
    string Status,
    bool RequiresHumanApproval,
    bool? ApprovedByHuman,
    DateTimeOffset CreatedAt,
    int TokensUsed
);

