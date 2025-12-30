using MediatR;

namespace IntelliPM.Application.Insights.Queries;

public record GetProjectInsightsQuery(int ProjectId, string? Status = null, string? AgentType = null) : IRequest<GetProjectInsightsResponse>;

public record GetProjectInsightsResponse(List<InsightDto> Insights, int Total);

public record InsightDto(
    int Id,
    string AgentType,
    string Category,
    string Title,
    string Description,
    string? Recommendation,
    decimal Confidence,
    string Priority,
    string Status,
    DateTimeOffset CreatedAt
);

