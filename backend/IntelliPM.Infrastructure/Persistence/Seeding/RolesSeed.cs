using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Persistence.Seeding;

/// <summary>
/// Documentation and seed data for system roles.
/// Version: 1.0
/// 
/// Note: Roles are defined as enums (GlobalRole and ProjectRole) and cannot be seeded
/// as database entities. This file documents the role definitions and their purposes.
/// </summary>
public static class RolesSeed
{
    public const string SeedName = "RolesSeed";
    public const string Version = "1.0";

    /// <summary>
    /// Global roles (organization-level permissions):
    /// - User: Standard user with limited permissions
    /// - Admin: Organization administrator (manages only their own organization)
    /// - SuperAdmin: System administrator (manages all organizations)
    /// </summary>
    public static class GlobalRoles
    {
        public const string User = "User";
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";
    }

    /// <summary>
    /// Project roles (project-level permissions):
    /// - ProductOwner: Owns product vision, prioritizes backlog, makes product decisions
    /// - ScrumMaster: Facilitates Scrum process, manages sprints, removes impediments
    /// - Developer: Develops features, writes code, completes tasks
    /// - Tester: Tests features, validates quality, approves releases
    /// - Viewer: Read-only access to project information
    /// - Manager: Manages resources, tracks progress, makes strategic decisions
    /// </summary>
    public static class ProjectRoles
    {
        public const string ProductOwner = "ProductOwner";
        public const string ScrumMaster = "ScrumMaster";
        public const string Developer = "Developer";
        public const string Tester = "Tester";
        public const string Viewer = "Viewer";
        public const string Manager = "Manager";
    }

    /// <summary>
    /// Logs role definitions (no database seeding needed as roles are enums).
    /// </summary>
    public static Task LogRoleDefinitionsAsync(ILogger logger)
    {
        logger.LogInformation("Role definitions (v{Version}):", Version);
        logger.LogInformation("Global Roles: {Roles}", string.Join(", ", new[] { GlobalRoles.User, GlobalRoles.Admin, GlobalRoles.SuperAdmin }));
        logger.LogInformation("Project Roles: {Roles}", string.Join(", ", new[] { 
            ProjectRoles.ProductOwner, 
            ProjectRoles.ScrumMaster, 
            ProjectRoles.Developer, 
            ProjectRoles.Tester, 
            ProjectRoles.Viewer, 
            ProjectRoles.Manager 
        }));
        return Task.CompletedTask;
    }
}

