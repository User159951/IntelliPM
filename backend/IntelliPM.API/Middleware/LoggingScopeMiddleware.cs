using IntelliPM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace IntelliPM.API.Middleware;

/// <summary>
/// Middleware that adds organization context and user information to the Serilog LogContext.
/// This ensures all logs within a request scope automatically include OrganizationId, UserId, and RequestPath.
/// Must be placed after authentication middleware so user context is available.
/// </summary>
public class LoggingScopeMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingScopeMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Scoped services (ICurrentUserService) are injected via method parameters
    /// because middleware is a singleton and cannot inject scoped services via constructor.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        // Get user and organization context
        var userId = currentUserService.GetUserId();
        var organizationId = currentUserService.GetOrganizationId();
        var requestPath = context.Request.Path.Value ?? string.Empty;

        // Add to Serilog LogContext for automatic inclusion in all logs
        using (LogContext.PushProperty("UserId", userId > 0 ? userId : (int?)null))
        using (LogContext.PushProperty("OrganizationId", organizationId > 0 ? organizationId : (int?)null))
        using (LogContext.PushProperty("RequestPath", requestPath))
        {
            await _next(context);
        }
    }
}
