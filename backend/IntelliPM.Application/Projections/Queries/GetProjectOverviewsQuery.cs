using MediatR;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Projections.Queries;

/// <summary>
/// Query to retrieve paginated list of project overview read models.
/// Supports filtering by organization and status for dashboard views.
/// </summary>
public record GetProjectOverviewsQuery : IRequest<PagedResponse<ProjectOverviewReadModelDto>>
{
    /// <summary>
    /// Optional: Filter by organization ID.
    /// </summary>
    public int? OrganizationId { get; init; }

    /// <summary>
    /// Optional: Filter by project status (Active, Archived).
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Page number (1-based). Default: 1
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page. Default: 20, Max: 100
    /// </summary>
    public int PageSize { get; init; } = 20;
}

