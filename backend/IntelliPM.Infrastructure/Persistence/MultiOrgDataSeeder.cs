using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.ValueObjects;
using IntelliPM.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Persistence;

/// <summary>
/// Seeds multi-organization test data:
/// - Organization A: 3 users (Admin, PM, Dev)
/// - Organization B: 3 users (Admin, PM, Dev)
/// - Projects, tasks, sprints for each organization
/// </summary>
public class MultiOrgDataSeeder
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly ILogger<MultiOrgDataSeeder> _logger;

    public MultiOrgDataSeeder(AppDbContext context, PasswordHasher passwordHasher, ILogger<MultiOrgDataSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting multi-organization data seeding...");

            // Check if multi-org data already exists
            var orgA = await _context.Organizations.FirstOrDefaultAsync(o => o.Name == "Organization A");
            var orgB = await _context.Organizations.FirstOrDefaultAsync(o => o.Name == "Organization B");

            if (orgA != null && orgB != null)
            {
                _logger.LogInformation("Multi-organization data already seeded. Skipping...");
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var password = "Password123!"; // Same password for all test users

            // ============================================
            // ORGANIZATION A
            // ============================================
            _logger.LogInformation("Creating Organization A...");
            var organizationA = new Organization
            {
                Name = "Organization A",
                CreatedAt = now
            };
            _context.Organizations.Add(organizationA);
            await _context.SaveChangesAsync();

            // Create users for Organization A
            var (hashA, saltA) = _passwordHasher.HashPassword(password);
            
            var adminA = new User
            {
                Username = "admin-orga",
                Email = "admin@orga.com",
                PasswordHash = hashA,
                PasswordSalt = saltA,
                FirstName = "Admin",
                LastName = "OrgA",
                GlobalRole = GlobalRole.Admin,
                IsActive = true,
                OrganizationId = organizationA.Id,
                CreatedAt = now
            };

            var (hashA2, saltA2) = _passwordHasher.HashPassword(password);
            var pmA = new User
            {
                Username = "pm-orga",
                Email = "pm@orga.com",
                PasswordHash = hashA2,
                PasswordSalt = saltA2,
                FirstName = "Project",
                LastName = "Manager A",
                GlobalRole = GlobalRole.User,
                IsActive = true,
                OrganizationId = organizationA.Id,
                CreatedAt = now
            };

            var (hashA3, saltA3) = _passwordHasher.HashPassword(password);
            var devA = new User
            {
                Username = "dev-orga",
                Email = "dev@orga.com",
                PasswordHash = hashA3,
                PasswordSalt = saltA3,
                FirstName = "Developer",
                LastName = "OrgA",
                GlobalRole = GlobalRole.User,
                IsActive = true,
                OrganizationId = organizationA.Id,
                CreatedAt = now
            };

            _context.Users.AddRange(adminA, pmA, devA);
            await _context.SaveChangesAsync();

            // Create project for Organization A
            var projectA = new Project
            {
                Name = "Project Alpha",
                Description = "Main project for Organization A - Web application development",
                Type = "Scrum",
                SprintDurationDays = 14,
                OwnerId = adminA.Id,
                OrganizationId = organizationA.Id,
                Status = "Active",
                CreatedAt = now,
                UpdatedAt = now
            };
            _context.Projects.Add(projectA);
            await _context.SaveChangesAsync();

            // Add project members for Organization A
            var memberA1 = new ProjectMember
            {
                ProjectId = projectA.Id,
                UserId = adminA.Id,
                Role = ProjectRole.ProductOwner,
                InvitedById = adminA.Id,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = now
            };

            var memberA2 = new ProjectMember
            {
                ProjectId = projectA.Id,
                UserId = pmA.Id,
                Role = ProjectRole.ScrumMaster,
                InvitedById = adminA.Id,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = now
            };

            var memberA3 = new ProjectMember
            {
                ProjectId = projectA.Id,
                UserId = devA.Id,
                Role = ProjectRole.Developer,
                InvitedById = adminA.Id,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = now
            };

            _context.ProjectMembers.AddRange(memberA1, memberA2, memberA3);
            await _context.SaveChangesAsync();

            // Create sprint for Organization A
            var sprintA = new Sprint
            {
                ProjectId = projectA.Id,
                OrganizationId = organizationA.Id,
                Number = 1,
                Goal = "Sprint 1: Setup and initial features",
                StartDate = now.AddDays(-7),
                EndDate = now.AddDays(7),
                Status = "InProgress",
                CreatedAt = now
            };
            _context.Sprints.Add(sprintA);
            await _context.SaveChangesAsync();

            // Create tasks for Organization A
            var taskA1 = new ProjectTask
            {
                ProjectId = projectA.Id,
                OrganizationId = organizationA.Id,
                Title = "Setup development environment",
                Description = "Configure local development environment with all required tools",
                Status = "Done",
                Priority = "High",
                StoryPoints = new StoryPoints(5),
                AssigneeId = devA.Id,
                SprintId = sprintA.Id,
                CreatedById = pmA.Id,
                CreatedAt = now.AddDays(-6),
                UpdatedAt = now.AddDays(-5)
            };

            var taskA2 = new ProjectTask
            {
                ProjectId = projectA.Id,
                OrganizationId = organizationA.Id,
                Title = "Design database schema",
                Description = "Create ERD and database schema for the application",
                Status = "InProgress",
                Priority = "High",
                StoryPoints = new StoryPoints(8),
                AssigneeId = devA.Id,
                SprintId = sprintA.Id,
                CreatedById = pmA.Id,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-3)
            };

            var taskA3 = new ProjectTask
            {
                ProjectId = projectA.Id,
                OrganizationId = organizationA.Id,
                Title = "Implement authentication",
                Description = "Build user authentication and authorization system",
                Status = "Todo",
                Priority = "Medium",
                StoryPoints = new StoryPoints(13),
                AssigneeId = devA.Id,
                CreatedById = pmA.Id,
                CreatedAt = now.AddDays(-4),
                UpdatedAt = now.AddDays(-4)
            };

            var taskA4 = new ProjectTask
            {
                ProjectId = projectA.Id,
                OrganizationId = organizationA.Id,
                Title = "Create project dashboard",
                Description = "Build main dashboard with project overview and metrics",
                Status = "Todo",
                Priority = "Medium",
                StoryPoints = new StoryPoints(8),
                CreatedById = pmA.Id,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-3)
            };

            _context.ProjectTasks.AddRange(taskA1, taskA2, taskA3, taskA4);
            await _context.SaveChangesAsync();

            // ============================================
            // ORGANIZATION B
            // ============================================
            _logger.LogInformation("Creating Organization B...");
            var organizationB = new Organization
            {
                Name = "Organization B",
                CreatedAt = now
            };
            _context.Organizations.Add(organizationB);
            await _context.SaveChangesAsync();

            // Create users for Organization B
            var (hashB, saltB) = _passwordHasher.HashPassword(password);
            
            var adminB = new User
            {
                Username = "admin-orgb",
                Email = "admin@orgb.com",
                PasswordHash = hashB,
                PasswordSalt = saltB,
                FirstName = "Admin",
                LastName = "OrgB",
                GlobalRole = GlobalRole.Admin,
                IsActive = true,
                OrganizationId = organizationB.Id,
                CreatedAt = now
            };

            var (hashB2, saltB2) = _passwordHasher.HashPassword(password);
            var pmB = new User
            {
                Username = "pm-orgb",
                Email = "pm@orgb.com",
                PasswordHash = hashB2,
                PasswordSalt = saltB2,
                FirstName = "Project",
                LastName = "Manager B",
                GlobalRole = GlobalRole.User,
                IsActive = true,
                OrganizationId = organizationB.Id,
                CreatedAt = now
            };

            var (hashB3, saltB3) = _passwordHasher.HashPassword(password);
            var devB = new User
            {
                Username = "dev-orgb",
                Email = "dev@orgb.com",
                PasswordHash = hashB3,
                PasswordSalt = saltB3,
                FirstName = "Developer",
                LastName = "OrgB",
                GlobalRole = GlobalRole.User,
                IsActive = true,
                OrganizationId = organizationB.Id,
                CreatedAt = now
            };

            _context.Users.AddRange(adminB, pmB, devB);
            await _context.SaveChangesAsync();

            // Create project for Organization B
            var projectB = new Project
            {
                Name = "Project Beta",
                Description = "Main project for Organization B - Mobile app development",
                Type = "Scrum",
                SprintDurationDays = 14,
                OwnerId = adminB.Id,
                OrganizationId = organizationB.Id,
                Status = "Active",
                CreatedAt = now,
                UpdatedAt = now
            };
            _context.Projects.Add(projectB);
            await _context.SaveChangesAsync();

            // Add project members for Organization B
            var memberB1 = new ProjectMember
            {
                ProjectId = projectB.Id,
                UserId = adminB.Id,
                Role = ProjectRole.ProductOwner,
                InvitedById = adminB.Id,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = now
            };

            var memberB2 = new ProjectMember
            {
                ProjectId = projectB.Id,
                UserId = pmB.Id,
                Role = ProjectRole.ScrumMaster,
                InvitedById = adminB.Id,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = now
            };

            var memberB3 = new ProjectMember
            {
                ProjectId = projectB.Id,
                UserId = devB.Id,
                Role = ProjectRole.Developer,
                InvitedById = adminB.Id,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = now
            };

            _context.ProjectMembers.AddRange(memberB1, memberB2, memberB3);
            await _context.SaveChangesAsync();

            // Create sprint for Organization B
            var sprintB = new Sprint
            {
                ProjectId = projectB.Id,
                OrganizationId = organizationB.Id,
                Number = 1,
                Goal = "Sprint 1: Foundation and core features",
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(9),
                Status = "InProgress",
                CreatedAt = now
            };
            _context.Sprints.Add(sprintB);
            await _context.SaveChangesAsync();

            // Create tasks for Organization B
            var taskB1 = new ProjectTask
            {
                ProjectId = projectB.Id,
                OrganizationId = organizationB.Id,
                Title = "Setup mobile development environment",
                Description = "Configure React Native development environment",
                Status = "Done",
                Priority = "High",
                StoryPoints = new StoryPoints(5),
                AssigneeId = devB.Id,
                SprintId = sprintB.Id,
                CreatedById = pmB.Id,
                CreatedAt = now.AddDays(-4),
                UpdatedAt = now.AddDays(-3)
            };

            var taskB2 = new ProjectTask
            {
                ProjectId = projectB.Id,
                OrganizationId = organizationB.Id,
                Title = "Design mobile UI/UX",
                Description = "Create wireframes and design mockups for mobile app",
                Status = "InProgress",
                Priority = "High",
                StoryPoints = new StoryPoints(8),
                AssigneeId = devB.Id,
                SprintId = sprintB.Id,
                CreatedById = pmB.Id,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-2)
            };

            var taskB3 = new ProjectTask
            {
                ProjectId = projectB.Id,
                OrganizationId = organizationB.Id,
                Title = "Implement navigation",
                Description = "Build navigation structure for the mobile app",
                Status = "Todo",
                Priority = "Medium",
                StoryPoints = new StoryPoints(13),
                AssigneeId = devB.Id,
                CreatedById = pmB.Id,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2)
            };

            var taskB4 = new ProjectTask
            {
                ProjectId = projectB.Id,
                OrganizationId = organizationB.Id,
                Title = "API integration",
                Description = "Integrate mobile app with backend API",
                Status = "Todo",
                Priority = "Medium",
                StoryPoints = new StoryPoints(8),
                CreatedById = pmB.Id,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1)
            };

            _context.ProjectTasks.AddRange(taskB1, taskB2, taskB3, taskB4);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Multi-organization data seeded successfully!");
            _logger.LogInformation("Organization A: {OrgAId} - Users: {AdminA}, {PmA}, {DevA}", 
                organizationA.Id, adminA.Id, pmA.Id, devA.Id);
            _logger.LogInformation("Organization B: {OrgBId} - Users: {AdminB}, {PmB}, {DevB}", 
                organizationB.Id, adminB.Id, pmB.Id, devB.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding multi-organization data");
            throw;
        }
    }
}

