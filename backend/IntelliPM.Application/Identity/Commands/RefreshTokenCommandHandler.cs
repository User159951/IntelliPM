using MediatR;
using IntelliPM.Application.Common.Interfaces;

namespace IntelliPM.Application.Identity.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (accessToken, refreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return new RefreshTokenResponse(accessToken, refreshToken);
    }
}

