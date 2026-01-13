using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Tests.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaskAsync = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Integration.Persistence;

/// <summary>
/// Integration tests to verify automatic tenant isolation via EF Core global query filters.
/// Tests that queries automatically filter by OrganizationId without manual .Where() clauses.
/// </summary>
public class TenantIsolationGlobalFilterTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private readonly AppDbContext _context;

    public TenantIsolationGlobalFilterTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Clear existing data
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Create organizations
        var org1 = new Organization { Id = 1, Name = "Org1", Code = "org1", CreatedAt = DateTimeOffset.UtcNow };
        var org2 = new Organization { Id = 2, Name = "Org2", Code = "org2", CreatedAt = DateTimeOffset.UtcNow };
        _context.Organizations.AddRange(org1, org2);

        // Create users
        var user1 = new User
        {
            Id = 1,
            Username = "user1",
            Email = "user1@org1.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var user2 = new User
        {
            Id = 2,
            Username = "user2",
            Email = "user2@org2.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 2,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        _context.Users.AddRange(user1, user2);

        // Bypass filter to insert data
        _context.BypassTenantFilter = true;

        // Create projects
        var project1 = new Project
        {
            Id = 1,
            Name = "Project1",
            OrganizationId = 1,
            OwnerId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var project2 = new Project
        {
            Id = 2,
            Name = "Project2",
            OrganizationId = 2,
            OwnerId = 2,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Projects.AddRange(project1, project2);

        // Create tasks
        var task1 = new ProjectTask
        {
            Id = 1,
            Title = "Task1",
            OrganizationId = 1,
            ProjectId = 1,
            CreatedById = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var task2 = new ProjectTask
        {
            Id = 2,
            Title = "Task2",
            OrganizationId = 2,
            ProjectId = 2,
            CreatedById = 2,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.ProjectTasks.AddRange(task1, task2);

        // Create sprints
        var sprint1 = new Sprint
        {
            Id = 1,
            ProjectId = 1,
            OrganizationId = 1,
            Number = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var sprint2 = new Sprint
        {
            Id = 2,
            ProjectId = 2,
            OrganizationId = 2,
            Number = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Sprints.AddRange(sprint1, sprint2);

        // Create teams
        var team1 = new Team
        {
            Id = 1,
            Name = "Team1",
            OrganizationId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var team2 = new Team
        {
            Id = 2,
            Name = "Team2",
            OrganizationId = 2,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Teams.AddRange(team1, team2);

        // Create defects
        var defect1 = new Defect
        {
            Id = 1,
            ProjectId = 1,
            OrganizationId = 1,
            Title = "Defect1",
            ReportedAt = DateTimeOffset.UtcNow
        };
        var defect2 = new Defect
        {
            Id = 2,
            ProjectId = 2,
            OrganizationId = 2,
            Title = "Defect2",
            ReportedAt = DateTimeOffset.UtcNow
        };
        _context.Defects.AddRange(defect1, defect2);

        // Create comments
        var comment1 = new Comment
        {
            Id = 1,
            OrganizationId = 1,
            EntityType = "Task",
            EntityId = 1,
            Content = "Comment1",
            AuthorId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var comment2 = new Comment
        {
            Id = 2,
            OrganizationId = 2,
            EntityType = "Task",
            EntityId = 2,
            Content = "Comment2",
            AuthorId = 2,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Comments.AddRange(comment1, comment2);

        // Create notifications
        var notification1 = new Notification
        {
            Id = 1,
            UserId = 1,
            OrganizationId = 1,
            Type = "task_assigned",
            Message = "Notification1",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var notification2 = new Notification
        {
            Id = 2,
            UserId = 2,
            OrganizationId = 2,
            Type = "task_assigned",
            Message = "Notification2",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Notifications.AddRange(notification1, notification2);

        _context.SaveChanges();
        _context.BypassTenantFilter = false;
    }

    [Fact]
    public async TaskAsync Queries_Automatically_Filter_By_OrganizationId_WithoutManualWhere()
    {
        // Arrange - Set organization context for Org1
        _context.CurrentOrganizationId = 1;
        _context.BypassTenantFilter = false;

        // Act - Query WITHOUT manual .Where(x => x.OrganizationId == ...)
        // The global query filter should automatically apply
        var projects = await _context.Projects.ToListAsync();
        var tasks = await _context.ProjectTasks.ToListAsync();
        var sprints = await _context.Sprints.ToListAsync();
        var teams = await _context.Teams.ToListAsync();
        var defects = await _context.Defects.ToListAsync();
        var comments = await _context.Comments.ToListAsync();
        var notifications = await _context.Notifications.ToListAsync();

        // Assert - All queries return only Org1 data automatically
        projects.Should().HaveCount(1);
        projects[0].OrganizationId.Should().Be(1);
        projects[0].Name.Should().Be("Project1");

        tasks.Should().HaveCount(1);
        tasks[0].OrganizationId.Should().Be(1);

        sprints.Should().HaveCount(1);
        sprints[0].OrganizationId.Should().Be(1);

        teams.Should().HaveCount(1);
        teams[0].OrganizationId.Should().Be(1);

        defects.Should().HaveCount(1);
        defects[0].OrganizationId.Should().Be(1);

        comments.Should().HaveCount(1);
        comments[0].OrganizationId.Should().Be(1);

        notifications.Should().HaveCount(1);
        notifications[0].OrganizationId.Should().Be(1);
    }

    [Fact]
    public async TaskAsync UserFromOrg1_CannotSee_Org2Data()
    {
        // Arrange - Set organization context for Org1
        _context.CurrentOrganizationId = 1;
        _context.BypassTenantFilter = false;

        // Act - Try to query data that belongs to Org2
        var org2Project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == 2);
        var org2Task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == 2);
        var org2Sprint = await _context.Sprints.FirstOrDefaultAsync(s => s.Id == 2);

        // Assert - Should return null (filtered out by global query filter)
        org2Project.Should().BeNull("Org1 user should not see Org2 projects");
        org2Task.Should().BeNull("Org1 user should not see Org2 tasks");
        org2Sprint.Should().BeNull("Org1 user should not see Org2 sprints");
    }

    [Fact]
    public async TaskAsync SuperAdmin_CanBypass_Filter()
    {
        // Arrange - Set bypass flag (simulating SuperAdmin)
        _context.CurrentOrganizationId = 1;
        _context.BypassTenantFilter = true;

        // Act - Query all data
        var projects = await _context.Projects.ToListAsync();
        var tasks = await _context.ProjectTasks.ToListAsync();

        // Assert - Should return data from all organizations
        projects.Should().HaveCount(2, "SuperAdmin should see all projects");
        tasks.Should().HaveCount(2, "SuperAdmin should see all tasks");
    }

    [Fact]
    public async TaskAsync NoOrganizationContext_ReturnsEmpty()
    {
        // Arrange - No organization context set
        _context.CurrentOrganizationId = null;
        _context.BypassTenantFilter = false;

        // Act - Query data
        var projects = await _context.Projects.ToListAsync();
        var tasks = await _context.ProjectTasks.ToListAsync();

        // Assert - Should return empty (filter excludes all when CurrentOrganizationId is null)
        projects.Should().BeEmpty("No organization context should return empty");
        tasks.Should().BeEmpty("No organization context should return empty");
    }

    public void Dispose()
    {
        _context?.Dispose();
        _scope?.Dispose();
    }
}
