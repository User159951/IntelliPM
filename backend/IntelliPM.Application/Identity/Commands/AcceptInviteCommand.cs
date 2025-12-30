using MediatR;

namespace IntelliPM.Application.Identity.Commands;

public record AcceptInviteCommand(
    string Token,
    string Password,
    string FirstName,
    string LastName
) : IRequest<AcceptInviteResponse>;

public record AcceptInviteResponse(
    int UserId,
    string Username,
    string Email,
    string AccessToken,
    string RefreshToken
);

