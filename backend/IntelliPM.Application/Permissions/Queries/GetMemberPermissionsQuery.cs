using MediatR;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Permissions.DTOs;

namespace IntelliPM.Application.Permissions.Queries;

/// <summary>
/// Query to get a paginated list of organization members with their permissions (Admin only - own organization).
/// </summary>
public record GetMemberPermissionsQuery : IRequest<PagedResponse<MemberPermissionDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
}

