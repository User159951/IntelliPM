using MediatR;

namespace IntelliPM.Application.Identity.Commands;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;

public record LoginResponse(
    int UserId,
    string Username,
    string Email,
    List<string> Roles,
    string AccessToken,
    string RefreshToken
);

