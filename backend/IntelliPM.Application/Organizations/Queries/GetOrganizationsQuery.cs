using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Query to get a paginated list of organizations (SuperAdmin only).
/// </summary>
public record GetOrganizationsQuery : IRequest<PagedResponse<OrganizationDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
}

/// <summary>
/// Query to get a single organization by ID (SuperAdmin only).
/// </summary>
public record GetOrganizationByIdQuery : IRequest<OrganizationDto>
{
    public int OrganizationId { get; init; }
}

/// <summary>
/// Query to get the current user's organization (Admin only).
/// </summary>
public record GetMyOrganizationQuery : IRequest<OrganizationDto>;

/// <summary>
/// DTO for organization information.
/// </summary>
public record OrganizationDto(
    int Id,
    string Name,
    string Code,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    int UserCount
);

