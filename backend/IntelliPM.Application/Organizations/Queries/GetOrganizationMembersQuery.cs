using MediatR;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Identity.DTOs;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Query to get a paginated list of organization members (Admin only - own organization).
/// </summary>
public record GetOrganizationMembersQuery : IRequest<PagedResponse<UserListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
}

