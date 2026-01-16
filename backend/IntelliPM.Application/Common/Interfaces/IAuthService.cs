namespace IntelliPM.Application.Common.Interfaces;

public record LoginResult(
    int UserId,
    string Username,
    string Email,
    string GlobalRole,
    string AccessToken,
    string RefreshToken
);

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct);
    Task<int> RegisterAsync(string username, string email, string password, string firstName, string lastName, CancellationToken ct);
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task RevokeTokenAsync(int userId, string refreshToken, CancellationToken ct);
}

