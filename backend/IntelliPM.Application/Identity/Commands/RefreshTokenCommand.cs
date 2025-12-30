using MediatR;

namespace IntelliPM.Application.Identity.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken);

