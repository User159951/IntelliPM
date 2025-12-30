using System.Security.Claims;

namespace IntelliPM.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(int userId, string username, string email, List<string> roles);
    string GenerateRefreshToken();
    bool ValidateToken(string token, out ClaimsPrincipal? principal);
}

