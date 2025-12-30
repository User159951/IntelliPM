namespace IntelliPM.Application.Common.Interfaces;

public interface IAuthService
{
    Task<(string AccessToken, string RefreshToken)> LoginAsync(string username, string password, CancellationToken ct);
    Task<int> RegisterAsync(string username, string email, string password, string firstName, string lastName, CancellationToken ct);
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task RevokeTokenAsync(int userId, string refreshToken, CancellationToken ct);
}

