using System.Security.Claims;
using System.Collections.Concurrent;

namespace IntelliPM.Tests.Infrastructure.TestAuthentication;

/// <summary>
/// Thread-safe context for storing test authentication claims.
/// Each test can set claims that will be used by TestAuthHandler.
/// </summary>
public static class TestAuthenticationContext
{
    private static readonly AsyncLocal<ClaimsPrincipal?> _currentPrincipal = new();

    /// <summary>
    /// Gets the current test principal for the current async context.
    /// </summary>
    public static ClaimsPrincipal? CurrentPrincipal
    {
        get => _currentPrincipal.Value;
        private set => _currentPrincipal.Value = value;
    }

    /// <summary>
    /// Sets the authentication claims for the current test context.
    /// </summary>
    /// <param name="claims">The claims to use for authentication</param>
    public static void SetClaims(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, TestAuthHandler.Scheme);
        CurrentPrincipal = new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Clears the authentication context for the current test.
    /// </summary>
    public static void Clear()
    {
        CurrentPrincipal = null;
    }
}
