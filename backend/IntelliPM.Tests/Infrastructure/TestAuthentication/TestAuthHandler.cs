using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace IntelliPM.Tests.Infrastructure.TestAuthentication;

/// <summary>
/// Test authentication handler that supports both test context and JWT validation.
/// 1. First checks TestAuthenticationContext for test-defined claims
/// 2. Falls back to JWT validation if a Bearer token is provided
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new const string Scheme = "TestScheme";
    private readonly IConfiguration _configuration;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, loggerFactory, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // First, check if test context has a principal set
        var principal = TestAuthenticationContext.CurrentPrincipal;
        if (principal != null)
        {
            var ticket = new AuthenticationTicket(principal, Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // No test context - try to validate JWT Bearer token
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            try
            {
                var secretKey = _configuration["Jwt:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    Logger.LogWarning("JWT SecretKey not configured for test authentication");
                    return Task.FromResult(AuthenticateResult.NoResult());
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var jwtTicket = new AuthenticationTicket(claimsPrincipal, Scheme);
                return Task.FromResult(AuthenticateResult.Success(jwtTicket));
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "JWT token validation failed");
                return Task.FromResult(AuthenticateResult.NoResult());
            }
        }

        // No authentication set - return no result (not failure, so other handlers can try)
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
