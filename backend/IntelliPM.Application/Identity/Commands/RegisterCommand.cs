using MediatR;

namespace IntelliPM.Application.Identity.Commands;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<RegisterResponse>;

public record RegisterResponse(int UserId, string Username, string Email, string AccessToken, string RefreshToken);

