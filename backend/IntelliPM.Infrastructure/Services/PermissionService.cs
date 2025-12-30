using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Implementation of IPermissionService that evaluates user permissions based on GlobalRole
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private const int CacheExpirationMinutes = 5;
    private const string CacheKeyPrefix = "user_permissions_";

    public PermissionService(AppDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken ct = default)
    {
        // Check cache first
        var cacheKey = $"{CacheKeyPrefix}{userId}";
        var cachedPermissions = await _cache.GetAsync<List<string>>(cacheKey, ct);
        if (cachedPermissions != null)
        {
            return cachedPermissions;
        }

        // Query User to get GlobalRole
        var user = await _context.Users
            .IgnoreQueryFilters() // Bypass any global query filters for permission lookup
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            return new List<string>();
        }

        // Query RolePermissions by Role, including Permission navigation
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.Role == user.GlobalRole)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Name)
            .ToListAsync(ct);

        // Cache the result for 5 minutes
        await _cache.SetAsync(cacheKey, rolePermissions, TimeSpan.FromMinutes(CacheExpirationMinutes), ct);

        return rolePermissions;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var permissions = await GetUserPermissionsAsync(userId, ct);
        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}

