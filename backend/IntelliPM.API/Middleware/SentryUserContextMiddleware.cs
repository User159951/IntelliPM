using IntelliPM.Application.Common.Interfaces;
using Sentry;
using System.Security.Claims;

namespace IntelliPM.API.Middleware;

/// <summary>
/// Middleware to capture user context (userId, organizationId) in Sentry scope
/// </summary>
public class SentryUserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SentryUserContextMiddleware> _logger;

    public SentryUserContextMiddleware(RequestDelegate next, ILogger<SentryUserContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        // Only capture user context if user is authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                // Get userId from JWT claims
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                {
                    // Set user context in Sentry scope
                    SentrySdk.ConfigureScope(scope =>
                    {
                        scope.User = new SentryUser
                        {
                            Id = userId.ToString(),
                            Username = context.User.FindFirst(ClaimTypes.Name)?.Value,
                            Email = context.User.FindFirst(ClaimTypes.Email)?.Value
                        };

                        // Get organizationId from service (may require DB lookup)
                        // Only try if we have a valid userId to avoid unnecessary DB calls
                        if (userId > 0)
                        {
                            try
                            {
                                var organizationId = currentUserService.GetOrganizationId();
                                if (organizationId > 0)
                                {
                                    scope.SetTag("organizationId", organizationId.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log but don't fail the request if organizationId lookup fails
                                _logger.LogWarning(ex, "Failed to get organizationId for Sentry context");
                            }
                        }

                        // Add additional context
                        scope.SetTag("userId", userId.ToString());
                    });
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the request if Sentry context setup fails
                _logger.LogWarning(ex, "Failed to set Sentry user context");
            }
        }

        await _next(context);
    }
}

