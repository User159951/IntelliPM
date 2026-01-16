using MediatR;
using IntelliPM.Application.Common.Interfaces;

namespace IntelliPM.Application.Identity.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginResult = await _authService.LoginAsync(
            request.Username, request.Password, cancellationToken);

        // Return GlobalRole in response (roles in token already include GlobalRole from AuthService)
        var roles = new List<string> { loginResult.GlobalRole };

        return new LoginResponse(
            loginResult.UserId,
            loginResult.Username,
            loginResult.Email,
            roles,
            loginResult.AccessToken,
            loginResult.RefreshToken
        );
    }
}

