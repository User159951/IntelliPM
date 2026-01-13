using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IntelliPM.API.Middleware;

/// <summary>
/// Middleware to set tenant context (CurrentOrganizationId and BypassTenantFilter) on AppDbContext.
/// This enables automatic tenant isolation via EF Core global query filters.
/// Must be placed after authentication middleware so user context is available.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(
        RequestDelegate next,
        ILogger<TenantMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(
        HttpContext context,
        AppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        // Set tenant context on DbContext for automatic query filtering
        var organizationId = currentUserService.GetOrganizationId();
        var isSuperAdmin = currentUserService.IsSuperAdmin();

        if (organizationId > 0)
        {
            dbContext.CurrentOrganizationId = organizationId;
            _logger.LogDebug(
                "Set tenant context: OrganizationId={OrganizationId}, BypassTenantFilter={BypassTenantFilter}",
                organizationId,
                isSuperAdmin);
        }
        else
        {
            // No organization ID - user not authenticated or not part of an organization
            // Set to null so filter will exclude all tenant entities
            dbContext.CurrentOrganizationId = null;
            _logger.LogDebug("No organization ID found for user, tenant filter will exclude all tenant entities");
        }

        // SuperAdmin can bypass tenant filter to see all organizations
        dbContext.BypassTenantFilter = isSuperAdmin;

        // Continue to next middleware
        await _next(context);
    }
}
