using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Identity.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(IAuthService authService, IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userId = await _authService.RegisterAsync(
            request.Username, request.Email, request.Password, request.FirstName, request.LastName, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(userId, request.Username, request.Email, new() { "Developer" });
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };
        var rtRepo = _unitOfWork.Repository<RefreshToken>();
        await rtRepo.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(userId, request.Username, request.Email, accessToken, refreshToken);
    }
}

