using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Implementation of IPermissionService that evaluates user permissions based on GlobalRole.
/// SECURITY: All permission checks are logged for audit purposes.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<PermissionService>? _logger;
    private const int CacheExpirationMinutes = 5;
    private const string CacheKeyPrefix = "user_permissions_";

    public PermissionService(
        AppDbContext context, 
        ICacheService cache,
        ILogger<PermissionService>? logger = null)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken ct = default)
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

    public async System.Threading.Tasks.Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            await LogPermissionCheckAsync(userId, permission, false, "Permission is null or empty", ct);
            return false;
        }

        var permissions = await GetUserPermissionsAsync(userId, ct);
        var hasPermission = permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        
        var reason = hasPermission 
            ? "Permission found in user's role permissions" 
            : "Permission not found in user's role permissions";
        
        await LogPermissionCheckAsync(userId, permission, hasPermission, reason, ct);
        
        return hasPermission;
    }

    /// <summary>
    /// Logs a permission check to audit log for security tracking.
    /// </summary>
    private async System.Threading.Tasks.Task LogPermissionCheckAsync(
        int userId, 
        string permission, 
        bool allowed, 
        string reason, 
        CancellationToken ct)
    {
        try
        {
            // Get user info for audit log
            var user = await _context.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            var auditLog = new AuditLog
            {
                UserId = userId > 0 ? userId : null,
                Action = allowed ? "PermissionCheck.Allowed" : "PermissionCheck.Denied",
                EntityType = "Permission",
                EntityId = null,
                EntityName = permission,
                Changes = JsonSerializer.Serialize(new
                {
                    Permission = permission,
                    UserId = userId,
                    UserEmail = user?.Email,
                    UserRole = user?.GlobalRole.ToString(),
                    OrganizationId = user?.OrganizationId,
                    Allowed = allowed,
                    Reason = reason,
                    Timestamp = DateTimeOffset.UtcNow
                }),
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - audit logging should not break permission checks
            _logger?.LogError(ex, 
                "Failed to log permission check audit: UserId={UserId}, Permission={Permission}, Allowed={Allowed}",
                userId, permission, allowed);
        }
    }
}

