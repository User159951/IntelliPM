using MediatR;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Users.Queries;

public record GetAllUsersQuery(
    int Page = 1,
    int PageSize = 20,
    GlobalRole? Role = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<PagedResponse<UserListDto>>;

public record UserListDto(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string GlobalRole,
    int OrganizationId,
    string OrganizationName,
    DateTimeOffset CreatedAt,
    bool IsActive,
    int ProjectCount,
    DateTimeOffset? LastLoginAt
);
