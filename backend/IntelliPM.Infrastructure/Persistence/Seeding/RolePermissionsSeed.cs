using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds role-permission mappings (permission matrix).
/// Version: 1.0
/// Defines which permissions each role has.
/// </summary>
public static class RolePermissionsSeed
{
    public const string SeedName = "RolePermissionsSeed";
    public const string Version = "1.0";

    /// <summary>
    /// Seeds role-permission mappings into the database.
    /// Idempotent: checks for existing mappings before inserting.
    /// </summary>
    public static async Task<int> SeedAsync(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding role permissions (v{Version})...", Version);

        // Get all permissions
        var allPermissions = await context.Permissions.ToListAsync();
        var permissionsDict = allPermissions.ToDictionary(p => p.Name, p => p.Id);

        // Get existing role permissions
        var existingRolePermissions = await context.RolePermissions
            .Select(rp => new { rp.Role, rp.PermissionId })
            .ToListAsync();

        var rolePermissions = new List<RolePermission>();
        var now = DateTimeOffset.UtcNow;

        // ============================================
        // SUPERADMIN ROLE: ALL PERMISSIONS
        // ============================================
        foreach (var permission in allPermissions)
        {
            if (!existingRolePermissions.Any(erp => erp.Role == GlobalRole.SuperAdmin && erp.PermissionId == permission.Id))
            {
                rolePermissions.Add(new RolePermission
                {
                    Role = GlobalRole.SuperAdmin,
                    PermissionId = permission.Id,
                    CreatedAt = now
                });
            }
        }

        // ============================================
        // ADMIN ROLE: ALL PERMISSIONS (same as SuperAdmin)
        // ============================================
        foreach (var permission in allPermissions)
        {
            if (!existingRolePermissions.Any(erp => erp.Role == GlobalRole.Admin && erp.PermissionId == permission.Id))
            {
                rolePermissions.Add(new RolePermission
                {
                    Role = GlobalRole.Admin,
                    PermissionId = permission.Id,
                    CreatedAt = now
                });
            }
        }

        // ============================================
        // USER ROLE: LIMITED PERMISSIONS
        // ============================================
        var userPermissions = new[]
        {
            // Projects - basic access
            "projects.view",
            "projects.create",
            "projects.edit",
            "projects.members.invite",
            "projects.settings.view",
            
            // Tasks - full access
            "tasks.view",
            "tasks.create",
            "tasks.update",
            "tasks.edit",
            "tasks.assign",
            "tasks.changeStatus",
            "tasks.comment",
            "tasks.attach",
            "tasks.view.dependencies",
            "tasks.dependencies.create",
            "tasks.dependencies.delete",
            "tasks.time.track",
            "tasks.estimate",
            
            // Sprints - view and participate
            "sprints.view",
            "sprints.create",
            "sprints.manage",
            "sprints.planning",
            "sprints.retrospective",
            
            // Backlog - full access
            "backlog.view",
            "backlog.create",
            "backlog.edit",
            "backlog.prioritize",
            "backlog.estimate",
            "backlog.refine",
            
            // Defects - full access
            "defects.view",
            "defects.create",
            "defects.edit",
            "defects.assign",
            "defects.resolve",
            "defects.verify",
            
            // Milestones - view and track
            "milestones.view",
            "milestones.track",
            
            // Releases - view only
            "releases.view",
            
            // Teams - view and availability
            "teams.view",
            "teams.view.availability",
            
            // Users - view own profile
            "users.view.profile",
            
            // Activity & Notifications
            "activity.view",
            "notifications.view",
            "notifications.markRead",
            "notifications.delete",
            
            // Search
            "search.use",
            "search.advanced",
            
            // Metrics & Insights - view only
            "metrics.view",
            "metrics.dashboard.view",
            "metrics.velocity.view",
            "metrics.burndown.view",
            "insights.view",
            "insights.acknowledge",
            "insights.feedback",
            
            // AI - use but limited approval
            "ai.use",
            "ai.decisions.view",
            
            // Risks - view and create
            "risks.view",
            "risks.create",
            "risks.edit",
            
            // Workflow - execute transitions
            "workflow.transitions.execute",
        };

        foreach (var permissionName in userPermissions)
        {
            if (permissionsDict.TryGetValue(permissionName, out var permissionId))
            {
                if (!existingRolePermissions.Any(erp => erp.Role == GlobalRole.User && erp.PermissionId == permissionId))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        Role = GlobalRole.User,
                        PermissionId = permissionId,
                        CreatedAt = now
                    });
                }
            }
        }

        if (rolePermissions.Any())
        {
            context.RolePermissions.AddRange(rolePermissions);
            await context.SaveChangesAsync();
            
            var superAdminCount = rolePermissions.Count(rp => rp.Role == GlobalRole.SuperAdmin);
            var adminCount = rolePermissions.Count(rp => rp.Role == GlobalRole.Admin);
            var userCount = rolePermissions.Count(rp => rp.Role == GlobalRole.User);
            
            logger.LogInformation(
                "Seeded {Total} new role permissions (SuperAdmin: {SuperAdminCount}, Admin: {AdminCount}, User: {UserCount})",
                rolePermissions.Count, superAdminCount, adminCount, userCount);
        }
        else
        {
            logger.LogInformation("All role permissions already exist");
        }

        return rolePermissions.Count;
    }
}

