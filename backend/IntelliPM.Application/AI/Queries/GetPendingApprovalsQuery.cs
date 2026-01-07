using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get pending AI decision approvals for an organization.
/// Returns decisions that require approval and are in Pending status.
/// </summary>
public record GetPendingApprovalsQuery : IRequest<PagedResponse<PendingApprovalDto>>
{
    public int? OrganizationId { get; init; } // null = all organizations (SuperAdmin only)
    public string? DecisionType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// DTO for pending approval information.
/// </summary>
public record PendingApprovalDto(
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
    DateTimeOffset CreatedAt,
    DateTimeOffset? ApprovalDeadline,
    bool IsExpired,
    int RequestedByUserId,
    string RequestedByUserName,
    int OrganizationId,
    string OrganizationName,
    int TokensUsed,
    decimal CostAccumulated
);

