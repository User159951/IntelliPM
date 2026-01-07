using MediatR;
using IntelliPM.Application.Reports.DTOs;

namespace IntelliPM.Application.Reports.Queries;

/// <summary>
/// Query to get AI decision report grouped by approver role.
/// </summary>
public record GetAIDecisionsByApproverRoleQuery : IRequest<List<AIDecisionRoleReportDto>>
{
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? RoleFilter { get; init; } // Optional role filter
    public int? OrganizationId { get; init; } // For multi-tenancy
}

