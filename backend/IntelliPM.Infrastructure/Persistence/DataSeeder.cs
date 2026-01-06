using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Persistence;

public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext context, PasswordHasher passwordHasher, ILogger<DataSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task SeedAsync()
    {
        try
        {
            // Check if already seeded
            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Database already seeded");
                return;
            }

            _logger.LogInformation("Seeding database...");

            // Seed Default Organization
            var defaultOrganization = new Organization
            {
                Name = "Default Organization",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Organizations.Add(defaultOrganization);
            await _context.SaveChangesAsync();

            var organizationId = defaultOrganization.Id;

            // Seed Users
            var (hash1, salt1) = _passwordHasher.HashPassword("Password123!");
            var (hash2, salt2) = _passwordHasher.HashPassword("Password123!");

            var user1 = new User
            {
                Username = "admin",
                Email = "admin@intellipm.com",
                PasswordHash = hash1,
                PasswordSalt = salt1,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                OrganizationId = organizationId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var user2 = new User
            {
                Username = "john",
                Email = "john@intellipm.com",
                PasswordHash = hash2,
                PasswordSalt = salt2,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                OrganizationId = organizationId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            // Seed Project
            var project = new Project
            {
                Name = "IntelliPM MVP",
                Description = "Initial MVP project for IntelliPM",
                Type = "Scrum",
                SprintDurationDays = 14,
                OwnerId = user1.Id,
                OrganizationId = organizationId,
                Status = "Active",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Seed Project Members
            var member1 = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = user1.Id,
                Role = Domain.Enums.ProjectRole.ProductOwner,
                InvitedById = user1.Id,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = DateTimeOffset.UtcNow
            };

            var member2 = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = user2.Id,
                Role = Domain.Enums.ProjectRole.Developer,
                InvitedById = user1.Id,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = DateTimeOffset.UtcNow
            };

            _context.ProjectMembers.AddRange(member1, member2);
            await _context.SaveChangesAsync();

            // Seed Epics, Features, Stories
            var epic1 = new Epic
            {
                ProjectId = project.Id,
                Title = "User Management",
                Description = "Epic for user management features",
                Priority = 90,
                Status = "Active",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Epics.Add(epic1);
            await _context.SaveChangesAsync();

            var feature1 = new Feature
            {
                ProjectId = project.Id,
                EpicId = epic1.Id,
                Title = "User Authentication",
                Description = "Implement JWT authentication",
                Priority = 95,
                StoryPoints = 8,
                DomainTag = "Identity",
                Status = "Backlog",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Features.Add(feature1);
            await _context.SaveChangesAsync();

            var stories = new[]
            {
                new UserStory
                {
                    ProjectId = project.Id,
                    FeatureId = feature1.Id,
                    Title = "User Login",
                    Description = "As a user, I want to login with username and password",
                    AcceptanceCriteria = "Given valid credentials, when I login, then I receive an access token",
                    Priority = 100,
                    StoryPoints = 5,
                    DomainTag = "Identity",
                    Status = "Backlog",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new UserStory
                {
                    ProjectId = project.Id,
                    FeatureId = feature1.Id,
                    Title = "User Registration",
                    Description = "As a user, I want to register with username, email, and password",
                    AcceptanceCriteria = "Given valid data, when I register, then my account is created",
                    Priority = 98,
                    StoryPoints = 3,
                    DomainTag = "Identity",
                    Status = "Backlog",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            _context.UserStories.AddRange(stories);
            await _context.SaveChangesAsync();

            // Seed Sprints
            var sprint1 = new Sprint
            {
                ProjectId = project.Id,
                OrganizationId = organizationId,
                Number = 1,
                Goal = "Implement authentication foundation",
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(14),
                Status = "Active",
                CreatedAt = DateTimeOffset.UtcNow
            };

            var sprint2 = new Sprint
            {
                ProjectId = project.Id,
                OrganizationId = organizationId,
                Number = 2,
                Goal = "Build backlog and sprint management",
                StartDate = DateTimeOffset.UtcNow.AddDays(14),
                EndDate = DateTimeOffset.UtcNow.AddDays(28),
                Status = "NotStarted",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Sprints.AddRange(sprint1, sprint2);
            await _context.SaveChangesAsync();

            // Seed Sprint Items
            var sprintItem1 = new SprintItem
            {
                SprintId = sprint1.Id,
                UserStoryId = stories[0].Id,
                SnapshotStoryPoints = stories[0].StoryPoints,
                Status = "InProgress"
            };

            var sprintItem2 = new SprintItem
            {
                SprintId = sprint1.Id,
                UserStoryId = stories[1].Id,
                SnapshotStoryPoints = stories[1].StoryPoints,
                Status = "TODO"
            };

            _context.SprintItems.AddRange(sprintItem1, sprintItem2);
            await _context.SaveChangesAsync();

            // Seed Risks
            var risks = new[]
            {
                new Risk
                {
                    ProjectId = project.Id,
                    Title = "LLaMA model performance",
                    Description = "LLaMA might be too slow for real-time agent execution",
                    Probability = 3,
                    Impact = 4,
                    MitigationPlan = "Implement caching and async processing",
                    OwnerId = user1.Id,
                    Status = "Open",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new Risk
                {
                    ProjectId = project.Id,
                    Title = "pgvector scaling",
                    Description = "Vector search might not scale with large document stores",
                    Probability = 2,
                    Impact = 3,
                    MitigationPlan = "Monitor performance and add indexes as needed",
                    OwnerId = user1.Id,
                    Status = "Open",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new Risk
                {
                    ProjectId = project.Id,
                    Title = "User adoption",
                    Description = "Users might not trust AI recommendations",
                    Probability = 4,
                    Impact = 5,
                    MitigationPlan = "Provide transparency and allow manual overrides",
                    OwnerId = user1.Id,
                    Status = "Open",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            _context.Risks.AddRange(risks);
            await _context.SaveChangesAsync();

            // Seed KPI Snapshot
            var kpiSnapshot = new KPISnapshot
            {
                SprintId = sprint1.Id,
                VelocityPoints = 8,
                CompletedPoints = 0,
                DefectCount = 0,
                LeadTimeDays = 3.5m,
                CycleTimeDays = 2.0m,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.KPISnapshots.Add(kpiSnapshot);
            await _context.SaveChangesAsync();

            // Seed DocumentStore (notes)
            var notes = new[]
            {
                new DocumentStore
                {
                    ProjectId = project.Id,
                    Type = "Note",
                    Content = "Team decided to use JWT for authentication instead of sessions for better scalability",
                    Metadata = "{\"author\": \"admin\", \"date\": \"2024-01-15\"}",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DocumentStore
                {
                    ProjectId = project.Id,
                    Type = "Decision",
                    Content = "Architecture decision: Use Clean Architecture with CQRS pattern for better separation of concerns",
                    Metadata = "{\"author\": \"admin\", \"date\": \"2024-01-10\"}",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DocumentStore
                {
                    ProjectId = project.Id,
                    Type = "Meeting",
                    Content = "Sprint Planning: Agreed to prioritize authentication and project setup. Target velocity: 8 story points.",
                    Metadata = "{\"date\": \"2024-01-20\", \"participants\": [\"admin\", \"john\"]}",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            _context.DocumentStores.AddRange(notes);
            await _context.SaveChangesAsync();

            // Seed Defects
            var defects = new[]
            {
                new Defect
                {
                    ProjectId = project.Id,
                    OrganizationId = organizationId,
                    UserStoryId = stories[0].Id,
                    SprintId = sprint1.Id,
                    Title = "Login button not responding on mobile",
                    Description = "The login button does not respond to touch events on iOS devices",
                    Severity = "High",
                    Status = "Open",
                    ReportedById = user2.Id,
                    FoundInEnvironment = "Production",
                    StepsToReproduce = "1. Open app on iOS\n2. Navigate to login\n3. Tap login button\n4. Nothing happens",
                    ReportedAt = DateTimeOffset.UtcNow.AddDays(-2)
                },
                new Defect
                {
                    ProjectId = project.Id,
                    OrganizationId = organizationId,
                    Title = "Password validation too strict",
                    Description = "Password requirements not clearly communicated to user",
                    Severity = "Medium",
                    Status = "InProgress",
                    ReportedById = user1.Id,
                    AssignedToId = user2.Id,
                    FoundInEnvironment = "Staging",
                    ReportedAt = DateTimeOffset.UtcNow.AddDays(-1)
                }
            };

            _context.Defects.AddRange(defects);
            await _context.SaveChangesAsync();

            // Seed Insights
            var insights = new[]
            {
                new Insight
                {
                    ProjectId = project.Id,
                    AgentType = "Product",
                    Category = "Opportunity",
                    Title = "High-value feature identified",
                    Description = "User authentication should be prioritized as it blocks multiple downstream features",
                    Recommendation = "Move authentication stories to top of backlog and allocate 2 developers",
                    Confidence = 0.88m,
                    Priority = "High",
                    Status = "New",
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-6)
                },
                new Insight
                {
                    ProjectId = project.Id,
                    AgentType = "Delivery",
                    Category = "Risk",
                    Title = "Sprint velocity declining",
                    Description = "Velocity has decreased by 15% over last 2 sprints",
                    Recommendation = "Review team capacity and consider reducing scope or extending sprint",
                    Confidence = 0.82m,
                    Priority = "Medium",
                    Status = "Acknowledged",
                    AcknowledgedById = user1.Id,
                    AcknowledgedAt = DateTimeOffset.UtcNow.AddHours(-2),
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-8)
                }
            };

            _context.Insights.AddRange(insights);
            await _context.SaveChangesAsync();

            // Seed Alerts
            var alerts = new[]
            {
                new Alert
                {
                    ProjectId = project.Id,
                    Type = "DefectSpike",
                    Severity = "Warning",
                    Title = "Defect count increased",
                    Message = "2 new high-severity defects reported in last 24 hours",
                    TriggerData = "{\"defectCount\": 2, \"severity\": \"High\", \"period\": \"24h\"}",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                }
            };

            _context.Alerts.AddRange(alerts);
            await _context.SaveChangesAsync();

            // Seed GlobalSettings
            var globalSettings = new[]
            {
                new GlobalSetting
                {
                    Key = "ProjectCreation.AllowedRoles",
                    Value = "Admin,ProductOwner",
                    Description = "Comma-separated list of roles allowed to create projects. Valid roles: Admin, ProductOwner, ScrumMaster",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            _context.GlobalSettings.AddRange(globalSettings);
            await _context.SaveChangesAsync();

            // Seed Permissions and RolePermissions
            await SeedPermissionsAsync();
            await SeedRolePermissionsAsync();

            _logger.LogInformation("Database seeded successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }

    public async System.Threading.Tasks.Task SeedPermissionsAsync()
    {
        _logger.LogInformation("Seeding permissions...");

        // Get existing permissions to avoid duplicates
        var existingPermissions = await _context.Permissions
            .Select(p => p.Name)
            .ToListAsync();

        var permissions = new[]
        {
            // Projects
            new Permission
            {
                Name = "projects.view",
                Description = "View projects",
                Category = "Projects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "projects.create",
                Description = "Create new projects",
                Category = "Projects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "projects.update",
                Description = "Update existing projects",
                Category = "Projects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "projects.edit",
                Description = "Edit projects (alias for projects.update)",
                Category = "Projects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "projects.members.invite",
                Description = "Invite members to projects",
                Category = "Projects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "projects.members.remove",
                Description = "Remove members from projects",
                Category = "Projects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "projects.members.changeRole",
                Description = "Change member roles in projects",
                Category = "Projects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "projects.delete",
                Description = "Delete projects",
                Category = "Projects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Users
            new Permission
            {
                Name = "users.view",
                Description = "View users",
                Category = "Users",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "users.invite",
                Description = "Invite new users",
                Category = "Users",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "users.create",
                Description = "Create new users",
                Category = "Users",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "users.manage",
                Description = "Manage users (activate/deactivate)",
                Category = "Users",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "users.update",
                Description = "Update user information",
                Category = "Users",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "users.delete",
                Description = "Delete users",
                Category = "Users",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Admin
            new Permission
            {
                Name = "admin.access",
                Description = "Access admin panel",
                Category = "Admin",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "admin.panel.view",
                Description = "View admin panel dashboard",
                Category = "Admin",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "admin.settings.update",
                Description = "Update global settings",
                Category = "Admin",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "admin.permissions.update",
                Description = "Update permissions and role mappings",
                Category = "Admin",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Tasks
            new Permission
            {
                Name = "tasks.view",
                Description = "View tasks",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "tasks.create",
                Description = "Create new tasks",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "tasks.update",
                Description = "Update existing tasks",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "tasks.edit",
                Description = "Edit tasks (alias for tasks.update)",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "tasks.assign",
                Description = "Assign tasks to users",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "tasks.comment",
                Description = "Comment on tasks",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "tasks.delete",
                Description = "Delete tasks",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "tasks.dependencies.create",
                Description = "Create task dependencies",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "tasks.dependencies.delete",
                Description = "Delete task dependencies",
                Category = "Tasks",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Milestones
            new Permission
            {
                Name = "milestones.view",
                Description = "View milestones",
                Category = "Milestones",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "milestones.create",
                Description = "Create new milestones",
                Category = "Milestones",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "milestones.edit",
                Description = "Edit milestones",
                Category = "Milestones",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "milestones.complete",
                Description = "Mark milestones as completed",
                Category = "Milestones",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "milestones.delete",
                Description = "Delete milestones",
                Category = "Milestones",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Releases
            new Permission
            {
                Name = "releases.view",
                Description = "View releases and release details",
                Category = "Releases",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "releases.create",
                Description = "Create new releases",
                Category = "Releases",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "releases.edit",
                Description = "Edit releases and manage sprints",
                Category = "Releases",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "releases.delete",
                Description = "Delete releases",
                Category = "Releases",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "releases.deploy",
                Description = "Deploy releases to production",
                Category = "Releases",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "releases.notes.edit",
                Description = "Generate and edit release notes and changelog",
                Category = "Releases",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "releases.quality-gates.approve",
                Description = "Approve quality gates for releases",
                Category = "Releases",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Sprints
            new Permission
            {
                Name = "sprints.view",
                Description = "View sprints",
                Category = "Sprints",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "sprints.create",
                Description = "Create new sprints",
                Category = "Sprints",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "sprints.update",
                Description = "Update existing sprints",
                Category = "Sprints",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "sprints.delete",
                Description = "Delete sprints",
                Category = "Sprints",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "sprints.manage",
                Description = "Manage sprints (start, complete, assign tasks)",
                Category = "Sprints",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Defects
            new Permission
            {
                Name = "defects.view",
                Description = "View defects",
                Category = "Defects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "defects.create",
                Description = "Create new defects",
                Category = "Defects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "defects.edit",
                Description = "Edit existing defects",
                Category = "Defects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "defects.delete",
                Description = "Delete defects",
                Category = "Defects",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Backlog
            new Permission
            {
                Name = "backlog.view",
                Description = "View backlog items",
                Category = "Backlog",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "backlog.create",
                Description = "Create backlog items",
                Category = "Backlog",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "backlog.edit",
                Description = "Edit backlog items",
                Category = "Backlog",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "backlog.delete",
                Description = "Delete backlog items",
                Category = "Backlog",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Teams
            new Permission
            {
                Name = "teams.view",
                Description = "View teams",
                Category = "Teams",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "teams.create",
                Description = "Create new teams",
                Category = "Teams",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "teams.edit",
                Description = "Edit teams (update capacity, etc.)",
                Category = "Teams",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Permission
            {
                Name = "teams.view.availability",
                Description = "View team availability and capacity",
                Category = "Teams",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Activity
            new Permission
            {
                Name = "activity.view",
                Description = "View activity feed",
                Category = "Activity",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Search
            new Permission
            {
                Name = "search.use",
                Description = "Use global search",
                Category = "Search",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Metrics
            new Permission
            {
                Name = "metrics.view",
                Description = "View project metrics and analytics",
                Category = "Metrics",
                CreatedAt = DateTimeOffset.UtcNow
            },
            // Insights
            new Permission
            {
                Name = "insights.view",
                Description = "View project insights",
                Category = "Insights",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        // Only add permissions that don't already exist
        var permissionsToAdd = permissions
            .Where(p => !existingPermissions.Contains(p.Name))
            .ToList();

        if (permissionsToAdd.Any())
        {
            _context.Permissions.AddRange(permissionsToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {permissionsToAdd.Count} new permissions (total: {permissions.Length}, existing: {existingPermissions.Count})");
        }
        else
        {
            _logger.LogInformation($"All {permissions.Length} permissions already exist");
        }
    }

    public async System.Threading.Tasks.Task SeedRolePermissionsAsync()
    {
        _logger.LogInformation("Seeding role permissions...");

        // Get all permissions
        var allPermissions = await _context.Permissions.ToListAsync();
        var permissionsDict = allPermissions.ToDictionary(p => p.Name, p => p.Id);

        // Get existing role permissions to avoid duplicates
        var existingRolePermissions = await _context.RolePermissions
            .Select(rp => new { rp.Role, rp.PermissionId })
            .ToListAsync();

        var rolePermissions = new List<RolePermission>();

        // SuperAdmin Role: ALL permissions (same as Admin)
        foreach (var permission in allPermissions)
        {
            // Only add if it doesn't already exist
            if (!existingRolePermissions.Any(erp => erp.Role == GlobalRole.SuperAdmin && erp.PermissionId == permission.Id))
            {
                rolePermissions.Add(new RolePermission
                {
                    Role = GlobalRole.SuperAdmin,
                    PermissionId = permission.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        // Admin Role: ALL permissions
        foreach (var permission in allPermissions)
        {
            // Only add if it doesn't already exist
            if (!existingRolePermissions.Any(erp => erp.Role == GlobalRole.Admin && erp.PermissionId == permission.Id))
            {
                rolePermissions.Add(new RolePermission
                {
                    Role = GlobalRole.Admin,
                    PermissionId = permission.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        // User Role: Limited permissions
        var userPermissions = new[]
        {
            "projects.view",
            "projects.create",
            "projects.edit",
            "projects.members.invite",
            "tasks.view",
            "tasks.create",
            "tasks.update",
            "tasks.edit",
            "tasks.assign",
            "tasks.comment",
            "tasks.dependencies.create",
            "tasks.dependencies.delete",
            "milestones.view",
            "milestones.create",
            "sprints.view",
            "sprints.create",
            "sprints.manage",
            "defects.view",
            "defects.create",
            "defects.edit",
            "backlog.view",
            "backlog.create",
            "backlog.edit",
            "teams.view",
            "teams.create",
            "teams.edit",
            "teams.view.availability",
            "users.view",
            // Releases permissions (basic view permission for all users)
            // Note: ProjectRole-specific permissions (ProductOwner, ScrumMaster, etc.) are handled
            // in application code, not in database RolePermission table which only supports GlobalRole
            "releases.view",
            // Activity, Search, Metrics, Insights permissions
            "activity.view",
            "search.use",
            "metrics.view",
            "insights.view"
        };

        foreach (var permissionName in userPermissions)
        {
            if (permissionsDict.TryGetValue(permissionName, out var permissionId))
            {
                // Only add if it doesn't already exist
                if (!existingRolePermissions.Any(erp => erp.Role == GlobalRole.User && erp.PermissionId == permissionId))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        Role = GlobalRole.User,
                        PermissionId = permissionId,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }
        }

        if (rolePermissions.Any())
        {
            _context.RolePermissions.AddRange(rolePermissions);
            await _context.SaveChangesAsync();
            var superAdminCount = rolePermissions.Count(rp => rp.Role == GlobalRole.SuperAdmin);
            var adminCount = rolePermissions.Count(rp => rp.Role == GlobalRole.Admin);
            var userCount = rolePermissions.Count(rp => rp.Role == GlobalRole.User);
            _logger.LogInformation($"Seeded {rolePermissions.Count} new role permissions (SuperAdmin: {superAdminCount}, Admin: {adminCount}, User: {userCount})");
        }
        else
        {
            _logger.LogInformation("All role permissions already exist");
        }
    }

    /// <summary>
    /// Seeds a development-only admin user for local development environments.
    /// This method is ONLY executed when isDevelopment is true.
    /// 
    /// IMPORTANT: This admin user is for LOCAL DEVELOPMENT ONLY.
    /// - Credentials MUST NEVER be reused in production.
    /// - This method MUST be called with isDevelopment=true ONLY in development environments.
    /// - The password is intentionally simple for dev convenience but should NEVER be used in production.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="passwordHasher">Password hasher service</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="isDevelopment">True if running in development environment, false otherwise</param>
    public static async System.Threading.Tasks.Task SeedDevelopmentAdminUserAsync(
        AppDbContext context,
        PasswordHasher passwordHasher,
        ILogger logger,
        bool isDevelopment)
    {
        // CRITICAL: Only run in development environment
        if (!isDevelopment)
        {
            logger.LogInformation("Dev admin seed skipped - not in development environment");
            return;
        }

        try
        {
            // Check if any admin user already exists
            var adminExists = await context.Users
                .AnyAsync(u => u.GlobalRole == GlobalRole.Admin);

            if (adminExists)
            {
                logger.LogInformation("Dev admin seed skipped - admin user already exists");
                return;
            }

            logger.LogInformation("Seeding development admin user...");

            // Ensure there is at least one Organization (use default organization)
            var organization = await SeedDefaultOrganizationAsync(context, logger);

            // DEV-ONLY password: "Admin123!" - This is intentionally simple for dev convenience
            // WARNING: This password MUST NEVER be used in production environments
            const string devPassword = "Admin123!";
            var (passwordHash, passwordSalt) = passwordHasher.HashPassword(devPassword);

            // Create dev admin user
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@local",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FirstName = "Dev",
                LastName = "Admin",
                GlobalRole = GlobalRole.Admin,
                IsActive = true,
                OrganizationId = organization.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Log WARNING with credentials (but NOT the password itself)
            logger.LogWarning(
                "=== DEVELOPMENT ADMIN USER CREATED ===" +
                "\nUsername: {Username}" +
                "\nEmail: {Email}" +
                "\nPassword: Admin123! (DEV ONLY - DO NOT USE IN PRODUCTION)" +
                "\nThis user is for local development only and will NOT be created in production.",
                adminUser.Username,
                adminUser.Email);

            logger.LogInformation("Development admin user seeded successfully with ID {UserId}", adminUser.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding development admin user");
            throw;
        }
    }

    /// <summary>
    /// Seeds the default organization if it doesn't exist (by Code).
    /// Idempotent: will not create duplicates.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>The default organization (existing or newly created)</returns>
    public static async System.Threading.Tasks.Task<Organization> SeedDefaultOrganizationAsync(
        AppDbContext context,
        ILogger logger)
    {
        try
        {
            const string defaultOrgCode = "default";
            const string defaultOrgName = "Default Organization";

            // Check if default organization already exists by Code
            var existingOrg = await context.Organizations
                .FirstOrDefaultAsync(o => o.Code == defaultOrgCode);

            if (existingOrg != null)
            {
                logger.LogInformation("Default organization already exists with Code '{Code}' and ID {OrganizationId}",
                    defaultOrgCode, existingOrg.Id);
                return existingOrg;
            }

            logger.LogInformation("Creating default organization with Code '{Code}'...", defaultOrgCode);

            var defaultOrganization = new Organization
            {
                Name = defaultOrgName,
                Code = defaultOrgCode,
                CreatedAt = DateTimeOffset.UtcNow
            };

            context.Organizations.Add(defaultOrganization);
            await context.SaveChangesAsync();

            logger.LogInformation("Default organization created successfully with ID {OrganizationId}",
                defaultOrganization.Id);

            return defaultOrganization;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding default organization");
            throw;
        }
    }

    /// <summary>
    /// Seeds a SuperAdmin user from configuration if none exists.
    /// Idempotent: will not create duplicates or overwrite existing SuperAdmin users.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="passwordHasher">Password hasher service</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="configuration">Configuration instance</param>
    public static async System.Threading.Tasks.Task SeedSuperAdminUserAsync(
        AppDbContext context,
        PasswordHasher passwordHasher,
        ILogger logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        try
        {
            // Check if SuperAdmin user already exists
            var superAdminExists = await context.Users
                .AnyAsync(u => u.GlobalRole == GlobalRole.SuperAdmin);

            if (superAdminExists)
            {
                logger.LogInformation("SuperAdmin user seed skipped - SuperAdmin user already exists");
                return;
            }

            // Get SuperAdmin configuration
            var superAdminEmail = configuration["SuperAdmin:Email"];
            var superAdminPassword = configuration["SuperAdmin:Password"];
            var superAdminUsername = configuration["SuperAdmin:Username"] ?? "superadmin";
            var superAdminFirstName = configuration["SuperAdmin:FirstName"] ?? "Super";
            var superAdminLastName = configuration["SuperAdmin:LastName"] ?? "Admin";

            // Validate required configuration
            if (string.IsNullOrWhiteSpace(superAdminEmail))
            {
                logger.LogWarning("SuperAdmin:Email not configured. Skipping SuperAdmin user seeding.");
                return;
            }

            if (string.IsNullOrWhiteSpace(superAdminPassword))
            {
                logger.LogWarning("SuperAdmin:Password not configured. Skipping SuperAdmin user seeding.");
                return;
            }

            logger.LogInformation("Seeding SuperAdmin user with email {Email}...", superAdminEmail);

            // Ensure default organization exists
            var defaultOrganization = await SeedDefaultOrganizationAsync(context, logger);

            // Hash password
            var (passwordHash, passwordSalt) = passwordHasher.HashPassword(superAdminPassword);

            // Create SuperAdmin user
            var superAdminUser = new User
            {
                Username = superAdminUsername,
                Email = superAdminEmail,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FirstName = superAdminFirstName,
                LastName = superAdminLastName,
                GlobalRole = GlobalRole.SuperAdmin,
                IsActive = true,
                OrganizationId = defaultOrganization.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            context.Users.Add(superAdminUser);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "SuperAdmin user seeded successfully with ID {UserId}, Email {Email}, Username {Username}",
                superAdminUser.Id, superAdminUser.Email, superAdminUser.Username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding SuperAdmin user");
            throw;
        }
    }

    /// <summary>
    /// Ensures OrganizationAIQuota exists for all organizations with sensible defaults.
    /// Idempotent: will not create duplicates.
    /// </summary>
    public static async System.Threading.Tasks.Task SeedOrganizationAIQuotasAsync(
        AppDbContext context,
        ILogger logger)
    {
        try
        {
            // Get all organizations that don't have a quota yet
            var organizationsWithoutQuota = await context.Organizations
                .Where(o => !context.OrganizationAIQuotas.Any(q => q.OrganizationId == o.Id))
                .ToListAsync();

            if (organizationsWithoutQuota.Count == 0)
            {
                logger.LogInformation("All organizations already have AI quota configurations");
                return;
            }

            logger.LogInformation("Seeding OrganizationAIQuota for {Count} organizations", organizationsWithoutQuota.Count);

            // Use Free tier defaults from AIQuotaConstants
            var defaultTokenLimit = Domain.Constants.AIQuotaConstants.DefaultLimits[Domain.Constants.AIQuotaConstants.Tiers.Free].MaxTokensPerPeriod;
            var defaultRequestLimit = Domain.Constants.AIQuotaConstants.DefaultLimits[Domain.Constants.AIQuotaConstants.Tiers.Free].MaxRequestsPerPeriod;

            foreach (var org in organizationsWithoutQuota)
            {
                var quota = new Domain.Entities.OrganizationAIQuota
                {
                    OrganizationId = org.Id,
                    MonthlyTokenLimit = defaultTokenLimit,
                    MonthlyRequestLimit = defaultRequestLimit,
                    ResetDayOfMonth = null, // Reset on first day of month
                    IsAIEnabled = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                context.OrganizationAIQuotas.Add(quota);
            }

            await context.SaveChangesAsync();

            logger.LogInformation(
                "OrganizationAIQuota seeded successfully for {Count} organizations",
                organizationsWithoutQuota.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding OrganizationAIQuota");
            throw;
        }
    }
}

