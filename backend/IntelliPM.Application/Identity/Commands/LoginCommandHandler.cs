using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Identity.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(IAuthService authService, IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (accessToken, refreshToken) = await _authService.LoginAsync(
            request.Username, request.Password, cancellationToken);

        // Retrieve user for response (roles are already in token from AuthService)
        var userRepo = _unitOfWork.Repository<User>();
        var user = userRepo.Query().FirstOrDefault(u => u.Username == request.Username || u.Email == request.Username);

        if (user == null)
            throw new UnauthorizedException("User not found");

        // Return GlobalRole in response (roles in token already include GlobalRole)
        var roles = new List<string> { user.GlobalRole.ToString() };

        return new LoginResponse(
            user.Id,
            user.Username,
            user.Email,
            roles,
            accessToken,
            refreshToken
        );
    }
}

