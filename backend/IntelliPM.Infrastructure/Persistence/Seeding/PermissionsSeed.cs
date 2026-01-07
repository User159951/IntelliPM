using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Persistence.Seeding;

/// <summary>
/// Comprehensive seed data for all system permissions.
/// Version: 1.0
/// Contains 100+ permissions organized by category.
/// </summary>
public static class PermissionsSeed
{
    public const string SeedName = "PermissionsSeed";
    public const string Version = "1.0";

    /// <summary>
    /// Seeds all permissions into the database.
    /// Idempotent: checks for existing permissions before inserting.
    /// </summary>
    public static async Task<int> SeedAsync(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding permissions (v{Version})...", Version);

        var existingPermissions = await context.Permissions
            .Select(p => p.Name)
            .ToListAsync();

        var permissions = GetAllPermissions();
        var permissionsToAdd = permissions
            .Where(p => !existingPermissions.Contains(p.Name))
            .ToList();

        if (permissionsToAdd.Any())
        {
            context.Permissions.AddRange(permissionsToAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} new permissions (total: {Total}, existing: {Existing})",
                permissionsToAdd.Count, permissions.Count, existingPermissions.Count);
        }
        else
        {
            logger.LogInformation("All {Total} permissions already exist", permissions.Count);
        }

        return permissionsToAdd.Count;
    }

    /// <summary>
    /// Gets all permissions organized by category.
    /// </summary>
    private static List<Permission> GetAllPermissions()
    {
        var now = DateTimeOffset.UtcNow;
        var permissions = new List<Permission>();

        // ============================================
        // PROJECTS CATEGORY (15 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "projects.view", Description = "View projects", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.create", Description = "Create new projects", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.update", Description = "Update existing projects", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.edit", Description = "Edit projects (alias for projects.update)", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.delete", Description = "Delete projects", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.archive", Description = "Archive/unarchive projects", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.clone", Description = "Clone existing projects", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.export", Description = "Export project data", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.import", Description = "Import project data", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.settings.view", Description = "View project settings", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.settings.update", Description = "Update project settings", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.members.view", Description = "View project members", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.members.invite", Description = "Invite members to projects", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.members.remove", Description = "Remove members from projects", Category = "Projects", CreatedAt = now },
            new Permission { Name = "projects.members.changeRole", Description = "Change member roles in projects", Category = "Projects", CreatedAt = now },
        });

        // ============================================
        // USERS CATEGORY (12 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "users.view", Description = "View users", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.create", Description = "Create new users", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.update", Description = "Update user information", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.edit", Description = "Edit users (alias for users.update)", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.delete", Description = "Delete users", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.manage", Description = "Manage users (activate/deactivate)", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.invite", Description = "Invite new users", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.view.profile", Description = "View user profiles", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.view.activity", Description = "View user activity logs", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.resetPassword", Description = "Reset user passwords", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.assignRole", Description = "Assign roles to users", Category = "Users", CreatedAt = now },
            new Permission { Name = "users.view.permissions", Description = "View user permissions", Category = "Users", CreatedAt = now },
        });

        // ============================================
        // ADMIN CATEGORY (10 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "admin.access", Description = "Access admin panel", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.panel.view", Description = "View admin panel dashboard", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.settings.view", Description = "View global settings", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.settings.update", Description = "Update global settings", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.permissions.view", Description = "View permissions and role mappings", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.permissions.update", Description = "Update permissions and role mappings", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.organizations.view", Description = "View organizations", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.organizations.manage", Description = "Manage organizations", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.audit.view", Description = "View audit logs", Category = "Admin", CreatedAt = now },
            new Permission { Name = "admin.data.seeding", Description = "Trigger data seeding operations", Category = "Admin", CreatedAt = now },
        });

        // ============================================
        // TASKS CATEGORY (15 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "tasks.view", Description = "View tasks", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.create", Description = "Create new tasks", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.update", Description = "Update existing tasks", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.edit", Description = "Edit tasks (alias for tasks.update)", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.delete", Description = "Delete tasks", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.assign", Description = "Assign tasks to users", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.changeStatus", Description = "Change task status", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.comment", Description = "Comment on tasks", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.attach", Description = "Attach files to tasks", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.view.dependencies", Description = "View task dependencies", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.dependencies.create", Description = "Create task dependencies", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.dependencies.delete", Description = "Delete task dependencies", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.time.track", Description = "Track time on tasks", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.estimate", Description = "Estimate task effort", Category = "Tasks", CreatedAt = now },
            new Permission { Name = "tasks.bulk.edit", Description = "Bulk edit tasks", Category = "Tasks", CreatedAt = now },
        });

        // ============================================
        // SPRINTS CATEGORY (10 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "sprints.view", Description = "View sprints", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.create", Description = "Create new sprints", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.update", Description = "Update existing sprints", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.edit", Description = "Edit sprints (alias for sprints.update)", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.delete", Description = "Delete sprints", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.manage", Description = "Manage sprints (start, complete, assign tasks)", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.start", Description = "Start sprints (EXCLUSIVE to ScrumMaster)", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.close", Description = "Close sprints (EXCLUSIVE to ScrumMaster)", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.planning", Description = "Participate in sprint planning", Category = "Sprints", CreatedAt = now },
            new Permission { Name = "sprints.retrospective", Description = "Participate in sprint retrospective", Category = "Sprints", CreatedAt = now },
        });

        // ============================================
        // BACKLOG CATEGORY (8 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "backlog.view", Description = "View backlog items", Category = "Backlog", CreatedAt = now },
            new Permission { Name = "backlog.create", Description = "Create backlog items", Category = "Backlog", CreatedAt = now },
            new Permission { Name = "backlog.edit", Description = "Edit backlog items", Category = "Backlog", CreatedAt = now },
            new Permission { Name = "backlog.delete", Description = "Delete backlog items", Category = "Backlog", CreatedAt = now },
            new Permission { Name = "backlog.prioritize", Description = "Prioritize backlog items", Category = "Backlog", CreatedAt = now },
            new Permission { Name = "backlog.estimate", Description = "Estimate backlog items", Category = "Backlog", CreatedAt = now },
            new Permission { Name = "backlog.refine", Description = "Refine backlog items", Category = "Backlog", CreatedAt = now },
            new Permission { Name = "backlog.bulk.edit", Description = "Bulk edit backlog items", Category = "Backlog", CreatedAt = now },
        });

        // ============================================
        // DEFECTS CATEGORY (10 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "defects.view", Description = "View defects", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.create", Description = "Create new defects", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.edit", Description = "Edit existing defects", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.delete", Description = "Delete defects", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.assign", Description = "Assign defects to users", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.resolve", Description = "Resolve defects", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.verify", Description = "Verify defect resolution", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.close", Description = "Close defects", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.reopen", Description = "Reopen closed defects", Category = "Defects", CreatedAt = now },
            new Permission { Name = "defects.export", Description = "Export defect reports", Category = "Defects", CreatedAt = now },
        });

        // ============================================
        // MILESTONES CATEGORY (8 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "milestones.view", Description = "View milestones", Category = "Milestones", CreatedAt = now },
            new Permission { Name = "milestones.create", Description = "Create new milestones", Category = "Milestones", CreatedAt = now },
            new Permission { Name = "milestones.edit", Description = "Edit milestones", Category = "Milestones", CreatedAt = now },
            new Permission { Name = "milestones.delete", Description = "Delete milestones", Category = "Milestones", CreatedAt = now },
            new Permission { Name = "milestones.complete", Description = "Mark milestones as completed", Category = "Milestones", CreatedAt = now },
            new Permission { Name = "milestones.validate", Description = "Validate and complete milestones (ProductOwner, ScrumMaster, Manager)", Category = "Milestones", CreatedAt = now },
            new Permission { Name = "milestones.track", Description = "Track milestone progress", Category = "Milestones", CreatedAt = now },
            new Permission { Name = "milestones.report", Description = "Generate milestone reports", Category = "Milestones", CreatedAt = now },
        });

        // ============================================
        // RELEASES CATEGORY (10 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "releases.view", Description = "View releases and release details", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.create", Description = "Create new releases", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.edit", Description = "Edit releases and manage sprints", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.delete", Description = "Delete releases", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.deploy", Description = "Deploy releases to production", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.notes.edit", Description = "Generate and edit release notes and changelog", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.quality-gates.approve", Description = "Approve quality gates for releases", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.approve", Description = "Approve releases for deployment (EXCLUSIVE to Tester/QA)", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.rollback", Description = "Rollback releases", Category = "Releases", CreatedAt = now },
            new Permission { Name = "releases.schedule", Description = "Schedule release deployments", Category = "Releases", CreatedAt = now },
        });

        // ============================================
        // QUALITY GATES CATEGORY (6 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "quality-gates.view", Description = "View quality gates", Category = "Quality Gates", CreatedAt = now },
            new Permission { Name = "quality-gates.create", Description = "Create quality gates", Category = "Quality Gates", CreatedAt = now },
            new Permission { Name = "quality-gates.edit", Description = "Edit quality gates", Category = "Quality Gates", CreatedAt = now },
            new Permission { Name = "quality-gates.delete", Description = "Delete quality gates", Category = "Quality Gates", CreatedAt = now },
            new Permission { Name = "quality-gates.validate", Description = "Validate quality gates (EXCLUSIVE to Tester/QA)", Category = "Quality Gates", CreatedAt = now },
            new Permission { Name = "quality-gates.approve", Description = "Approve quality gate results", Category = "Quality Gates", CreatedAt = now },
        });

        // ============================================
        // TEAMS CATEGORY (8 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "teams.view", Description = "View teams", Category = "Teams", CreatedAt = now },
            new Permission { Name = "teams.create", Description = "Create new teams", Category = "Teams", CreatedAt = now },
            new Permission { Name = "teams.edit", Description = "Edit teams (update capacity, etc.)", Category = "Teams", CreatedAt = now },
            new Permission { Name = "teams.delete", Description = "Delete teams", Category = "Teams", CreatedAt = now },
            new Permission { Name = "teams.view.availability", Description = "View team availability and capacity", Category = "Teams", CreatedAt = now },
            new Permission { Name = "teams.members.add", Description = "Add members to teams", Category = "Teams", CreatedAt = now },
            new Permission { Name = "teams.members.remove", Description = "Remove members from teams", Category = "Teams", CreatedAt = now },
            new Permission { Name = "teams.capacity.update", Description = "Update team capacity", Category = "Teams", CreatedAt = now },
        });

        // ============================================
        // METRICS & ANALYTICS CATEGORY (8 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "metrics.view", Description = "View project metrics and analytics", Category = "Metrics", CreatedAt = now },
            new Permission { Name = "metrics.export", Description = "Export metrics data", Category = "Metrics", CreatedAt = now },
            new Permission { Name = "metrics.dashboard.view", Description = "View metrics dashboards", Category = "Metrics", CreatedAt = now },
            new Permission { Name = "metrics.dashboard.customize", Description = "Customize metrics dashboards", Category = "Metrics", CreatedAt = now },
            new Permission { Name = "metrics.reports.generate", Description = "Generate metric reports", Category = "Metrics", CreatedAt = now },
            new Permission { Name = "metrics.velocity.view", Description = "View velocity metrics", Category = "Metrics", CreatedAt = now },
            new Permission { Name = "metrics.burndown.view", Description = "View burndown charts", Category = "Metrics", CreatedAt = now },
            new Permission { Name = "metrics.trends.view", Description = "View trend analysis", Category = "Metrics", CreatedAt = now },
        });

        // ============================================
        // INSIGHTS CATEGORY (6 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "insights.view", Description = "View project insights", Category = "Insights", CreatedAt = now },
            new Permission { Name = "insights.acknowledge", Description = "Acknowledge insights", Category = "Insights", CreatedAt = now },
            new Permission { Name = "insights.dismiss", Description = "Dismiss insights", Category = "Insights", CreatedAt = now },
            new Permission { Name = "insights.export", Description = "Export insights", Category = "Insights", CreatedAt = now },
            new Permission { Name = "insights.feedback", Description = "Provide feedback on insights", Category = "Insights", CreatedAt = now },
            new Permission { Name = "insights.configure", Description = "Configure insight generation", Category = "Insights", CreatedAt = now },
        });

        // ============================================
        // AI CATEGORY (10 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "ai.use", Description = "Use AI agents and features", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.decisions.view", Description = "View AI decisions", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.decisions.approve", Description = "Approve AI decisions", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.decisions.reject", Description = "Reject AI decisions", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.quota.view", Description = "View AI quota usage", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.quota.manage", Description = "Manage AI quotas", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.policies.view", Description = "View AI approval policies", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.policies.manage", Description = "Manage AI approval policies", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.agents.configure", Description = "Configure AI agents", Category = "AI", CreatedAt = now },
            new Permission { Name = "ai.audit.view", Description = "View AI decision audit logs", Category = "AI", CreatedAt = now },
        });

        // ============================================
        // ACTIVITY & NOTIFICATIONS CATEGORY (6 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "activity.view", Description = "View activity feed", Category = "Activity", CreatedAt = now },
            new Permission { Name = "activity.export", Description = "Export activity logs", Category = "Activity", CreatedAt = now },
            new Permission { Name = "notifications.view", Description = "View notifications", Category = "Notifications", CreatedAt = now },
            new Permission { Name = "notifications.manage", Description = "Manage notification preferences", Category = "Notifications", CreatedAt = now },
            new Permission { Name = "notifications.markRead", Description = "Mark notifications as read", Category = "Notifications", CreatedAt = now },
            new Permission { Name = "notifications.delete", Description = "Delete notifications", Category = "Notifications", CreatedAt = now },
        });

        // ============================================
        // SEARCH CATEGORY (2 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "search.use", Description = "Use global search", Category = "Search", CreatedAt = now },
            new Permission { Name = "search.advanced", Description = "Use advanced search features", Category = "Search", CreatedAt = now },
        });

        // ============================================
        // RISKS CATEGORY (8 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "risks.view", Description = "View risks", Category = "Risks", CreatedAt = now },
            new Permission { Name = "risks.create", Description = "Create risks", Category = "Risks", CreatedAt = now },
            new Permission { Name = "risks.edit", Description = "Edit risks", Category = "Risks", CreatedAt = now },
            new Permission { Name = "risks.delete", Description = "Delete risks", Category = "Risks", CreatedAt = now },
            new Permission { Name = "risks.mitigate", Description = "Mitigate risks", Category = "Risks", CreatedAt = now },
            new Permission { Name = "risks.assign", Description = "Assign risk owners", Category = "Risks", CreatedAt = now },
            new Permission { Name = "risks.close", Description = "Close risks", Category = "Risks", CreatedAt = now },
            new Permission { Name = "risks.report", Description = "Generate risk reports", Category = "Risks", CreatedAt = now },
        });

        // ============================================
        // WORKFLOW CATEGORY (6 permissions)
        // ============================================
        permissions.AddRange(new[]
        {
            new Permission { Name = "workflow.rules.view", Description = "View workflow transition rules", Category = "Workflow", CreatedAt = now },
            new Permission { Name = "workflow.rules.manage", Description = "Manage workflow transition rules", Category = "Workflow", CreatedAt = now },
            new Permission { Name = "workflow.audit.view", Description = "View workflow transition audit logs", Category = "Workflow", CreatedAt = now },
            new Permission { Name = "workflow.transitions.execute", Description = "Execute workflow transitions", Category = "Workflow", CreatedAt = now },
            new Permission { Name = "workflow.override", Description = "Override workflow rules (Admin only)", Category = "Workflow", CreatedAt = now },
            new Permission { Name = "workflow.configure", Description = "Configure workflow settings", Category = "Workflow", CreatedAt = now },
        });

        return permissions;
    }
}

