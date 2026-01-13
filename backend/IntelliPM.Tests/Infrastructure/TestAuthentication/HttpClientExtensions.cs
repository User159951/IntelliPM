using System.Net.Http.Headers;
using System.Security.Claims;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Tests.Infrastructure.TestAuthentication;

/// <summary>
/// Extension methods for HttpClient to easily authenticate in tests.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Authenticates the HTTP client with the specified user claims.
    /// </summary>
    /// <param name="client">The HTTP client to authenticate</param>
    /// <param name="userId">The user ID</param>
    /// <param name="username">The username</param>
    /// <param name="email">The email address</param>
    /// <param name="organizationId">Optional organization ID (not in JWT, but can be set for testing)</param>
    /// <param name="role">Optional global role</param>
    /// <param name="additionalRoles">Optional additional roles</param>
    public static void AuthenticateAs(
        this HttpClient client,
        int userId,
        string username,
        string email,
        int? organizationId = null,
        GlobalRole? role = null,
        params string[] additionalRoles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email)
        };

        if (organizationId.HasValue)
        {
            claims.Add(new Claim("OrganizationId", organizationId.Value.ToString()));
        }

        if (role.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Value.ToString()));
        }

        foreach (var additionalRole in additionalRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, additionalRole));
        }

        TestAuthenticationContext.SetClaims(claims);

        // Set a dummy authorization header to trigger authentication
        // The TestAuthHandler will ignore the actual token value
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "test-token");
    }

    /// <summary>
    /// Authenticates the HTTP client as a SuperAdmin user.
    /// </summary>
    /// <param name="client">The HTTP client to authenticate</param>
    /// <param name="userId">Optional user ID (defaults to 1)</param>
    /// <param name="organizationId">Optional organization ID</param>
    public static void AuthenticateAsSuperAdmin(
        this HttpClient client,
        int userId = 1,
        int? organizationId = null)
    {
        client.AuthenticateAs(
            userId: userId,
            username: "superadmin",
            email: "superadmin@test.com",
            organizationId: organizationId,
            role: GlobalRole.SuperAdmin);
    }

    /// <summary>
    /// Clears authentication for the HTTP client.
    /// </summary>
    /// <param name="client">The HTTP client</param>
    public static void ClearAuthentication(this HttpClient client)
    {
        TestAuthenticationContext.Clear();
        client.DefaultRequestHeaders.Authorization = null;
    }
}
