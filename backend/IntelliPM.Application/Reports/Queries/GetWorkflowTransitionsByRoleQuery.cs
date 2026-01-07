using MediatR;
using IntelliPM.Application.Reports.DTOs;

namespace IntelliPM.Application.Reports.Queries;

/// <summary>
/// Query to get workflow transition report grouped by user role.
/// </summary>
public record GetWorkflowTransitionsByRoleQuery : IRequest<List<WorkflowRoleReportDto>>
{
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? RoleFilter { get; init; } // Optional role filter
    public string? EntityTypeFilter { get; init; } // Optional entity type filter (task, sprint, project, etc.)
    public int? OrganizationId { get; init; } // For multi-tenancy
}

