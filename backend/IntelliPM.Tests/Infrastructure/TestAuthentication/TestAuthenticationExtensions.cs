using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliPM.Tests.Infrastructure.TestAuthentication;

/// <summary>
/// Extension methods for configuring test authentication in WebApplicationFactory.
/// </summary>
public static class TestAuthenticationExtensions
{
    /// <summary>
    /// Configures test authentication by adding TestAuthHandler and making it the default scheme.
    /// This should be called in ConfigureServices of WebApplicationFactory.
    /// Overrides the default authentication scheme from Program.cs to use TestScheme instead of Bearer.
    /// </summary>
    /// <param name="services">The service collection</param>
    public static void AddTestAuthentication(this IServiceCollection services)
    {
        // Post-configure authentication to override the default scheme set in Program.cs
        // This runs after Program.cs configuration, so it will override the default
        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultScheme = TestAuthHandler.Scheme;
            options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
            options.DefaultChallengeScheme = TestAuthHandler.Scheme;
        });

        // Add test authentication scheme
        // This will be added alongside the existing Bearer scheme from Program.cs
        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.Scheme, 
                options => { });
    }
}
