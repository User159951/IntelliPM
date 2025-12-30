using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get AI decision logs across all organizations (Admin only).
/// </summary>
public record GetAllAIDecisionLogsQuery : IRequest<PagedResponse<AIDecisionLogDto>>
{
    public int? OrganizationId { get; init; }
    public string? DecisionType { get; init; }
    public string? AgentType { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

