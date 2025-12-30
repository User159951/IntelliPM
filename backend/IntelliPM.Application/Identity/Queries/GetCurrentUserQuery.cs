using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Identity.Queries;

public record GetCurrentUserQuery(int UserId) : IRequest<CurrentUserDto>;

public record CurrentUserDto(
    int UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    GlobalRole GlobalRole,
    int OrganizationId,
    string[] Permissions
);

