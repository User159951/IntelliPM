using System.Security.Claims;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Implementation of ICurrentUserService that extracts user information from HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;
    private int? _cachedOrganizationId;
    private GlobalRole? _cachedGlobalRole;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public int GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return 0;
        }
        return userId;
    }

    public int GetOrganizationId()
    {
        // Cache the organization ID for the lifetime of the service (scoped)
        if (_cachedOrganizationId.HasValue)
        {
            return _cachedOrganizationId.Value;
        }

        var userId = GetUserId();
        if (userId == 0)
        {
            _cachedOrganizationId = 0;
            return 0;
        }

        // Query User table to get OrganizationId
        // Note: This query will be filtered by the global query filter if it exists
        // So we need to use IgnoreQueryFilters() to bypass it for this lookup
        // Use AsNoTracking() for read-only query
        var user = _context.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            _cachedOrganizationId = 0;
            return 0;
        }

        _cachedOrganizationId = user.OrganizationId;
        return user.OrganizationId;
    }

    public GlobalRole GetGlobalRole()
    {
        // Cache the global role for the lifetime of the service (scoped)
        if (_cachedGlobalRole.HasValue)
        {
            return _cachedGlobalRole.Value;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            _cachedGlobalRole = GlobalRole.User;
            return GlobalRole.User;
        }

        // Check roles from JWT claims
        if (user.IsInRole("SuperAdmin"))
        {
            _cachedGlobalRole = GlobalRole.SuperAdmin;
            return GlobalRole.SuperAdmin;
        }

        if (user.IsInRole("Admin"))
        {
            _cachedGlobalRole = GlobalRole.Admin;
            return GlobalRole.Admin;
        }

        _cachedGlobalRole = GlobalRole.User;
        return GlobalRole.User;
    }

    public bool IsAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return false;
        }

        // Admin and SuperAdmin both have admin privileges
        return user.IsInRole("Admin") || user.IsInRole("SuperAdmin");
    }

    /// <summary>
    /// Checks if the current user is a SuperAdmin
    /// </summary>
    /// <returns>True if user has "SuperAdmin" role, false otherwise</returns>
    public bool IsSuperAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return false;
        }

        return user.IsInRole("SuperAdmin");
    }

    public string? GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        // Get correlation ID from HttpContext.Items (set by CorrelationIdMiddleware)
        // Use the same key constant as CorrelationIdMiddleware
        const string CorrelationIdItemKey = "CorrelationId";
        if (httpContext.Items.TryGetValue(CorrelationIdItemKey, out var correlationIdObj) &&
            correlationIdObj is string correlationId)
        {
            return correlationId;
        }

        return null;
    }
}

