using MediatR;
using IntelliPM.Application.Reports.DTOs;

namespace IntelliPM.Application.Reports.Queries;

/// <summary>
/// Query to get activity report grouped by user role.
/// </summary>
public record GetActivityByRoleQuery : IRequest<List<RoleActivityReportDto>>
{
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? RoleFilter { get; init; } // Optional role filter (ProjectRole or GlobalRole)
    public string? ActionTypeFilter { get; init; } // Optional action type filter
    public int? OrganizationId { get; init; } // For multi-tenancy
}

