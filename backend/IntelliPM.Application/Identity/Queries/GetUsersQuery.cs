using MediatR;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Identity.DTOs;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Identity.Queries;

/// <summary>
/// Query to retrieve a paginated list of users with filtering, sorting, and search capabilities.
/// Admin-only access.
/// </summary>
public record GetUsersQuery : IRequest<PagedResponse<UserListDto>>
{
    /// <summary>
    /// Page number (1-based). Default: 1
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page. Default: 20, Max: 100
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Filter by global role (Admin or User). Optional.
    /// </summary>
    public GlobalRole? Role { get; init; }

    /// <summary>
    /// Filter by active status. Optional.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Field to sort by. Valid values: Username, Email, CreatedAt, LastLoginAt, Role, IsActive. Default: CreatedAt
    /// </summary>
    public string? SortField { get; init; }

    /// <summary>
    /// Sort in descending order. Default: false (ascending)
    /// </summary>
    public bool SortDescending { get; init; } = false;

    /// <summary>
    /// Search term to filter users by username, email, firstName, or lastName (case-insensitive). Optional.
    /// </summary>
    public string? SearchTerm { get; init; }
}

