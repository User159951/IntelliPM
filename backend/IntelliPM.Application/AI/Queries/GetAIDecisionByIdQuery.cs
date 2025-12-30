using MediatR;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get a specific AI decision by ID.
/// </summary>
public record GetAIDecisionByIdQuery : IRequest<AIDecisionLogDetailDto?>
{
    public Guid DecisionId { get; init; }
    public int OrganizationId { get; init; }
}

/// <summary>
/// Detailed DTO for AI decision log with full information.
/// </summary>
public record AIDecisionLogDetailDto(
    Guid DecisionId,
    string DecisionType,
    string AgentType,
    string EntityType,
    int EntityId,
    string EntityName,
    string Question,
    string Decision,
    string Reasoning,
    decimal ConfidenceScore,
    string ModelName,
    string ModelVersion,
    int TokensUsed,
    int PromptTokens,
    int CompletionTokens,
    string Status,
    bool RequiresHumanApproval,
    bool? ApprovedByHuman,
    int? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    string? ApprovalNotes,
    bool WasApplied,
    DateTimeOffset? AppliedAt,
    string? ActualOutcome,
    DateTimeOffset CreatedAt,
    int ExecutionTimeMs,
    bool IsSuccess,
    string? ErrorMessage
);

