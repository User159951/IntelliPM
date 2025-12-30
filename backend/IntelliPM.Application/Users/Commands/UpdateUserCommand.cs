using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Users.Commands;

public record UpdateUserCommand(
    int UserId,
    string? FirstName,
    string? LastName,
    string? Email,
    GlobalRole? GlobalRole
) : IRequest<UpdateUserResponse>;

public record UpdateUserResponse(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string GlobalRole
);

