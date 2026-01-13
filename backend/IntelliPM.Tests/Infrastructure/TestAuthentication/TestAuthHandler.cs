using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace IntelliPM.Tests.Infrastructure.TestAuthentication;

/// <summary>
/// Test authentication handler that bypasses JWT validation.
/// Reads claims from TestAuthenticationContext instead of validating tokens.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new const string Scheme = "TestScheme";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, loggerFactory, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Read claims from test context
        var principal = TestAuthenticationContext.CurrentPrincipal;

        if (principal == null)
        {
            // No authentication set - return no result (not failure, so other handlers can try)
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Create authentication ticket with the test principal
        var ticket = new AuthenticationTicket(principal, Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
