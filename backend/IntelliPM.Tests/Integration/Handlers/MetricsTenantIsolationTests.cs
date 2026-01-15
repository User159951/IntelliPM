using FluentAssertions;
using IntelliPM.Application.Queries.Metrics;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.ValueObjects;
using IntelliPM.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskAsync = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.Handlers;

/// <summary>
/// Integration tests to verify tenant isolation in Metrics query handlers.
/// Ensures that metrics are filtered by OrganizationId and users can only see their own organization's data.
/// </summary>
public class MetricsTenantIsolationTests : IClassFixture<AIAgentHandlerTestFactory>, IDisposable
{
    private readonly AIAgentHandlerTestFactory _factory;
    private readonly IServiceScope _seedScope;
    private readonly AppDbContext _seedDbContext;

    private Organization _org1 = null!;
    private Organization _org2 = null!;
    private User _user1 = null!;
    private User _user2 = null!;
    private Project _project1 = null!;
    private Project _project2 = null!;
    private Sprint _sprint1Completed = null!;
    private Sprint _sprint2Completed = null!;
    private ProjectTask _task1Todo = null!;
    private ProjectTask _task1InProgress = null!;
    private ProjectTask _task1Done = null!;
    private ProjectTask _task2Todo = null!;
    private ProjectTask _task2Done = null!;
    private Defect _defect1Open = null!;
    private Defect _defect2Open = null!;

    public MetricsTenantIsolationTests(AIAgentHandlerTestFactory factory)
    {
        _factory = factory;
        _seedScope = factory.Services.CreateScope();
        _seedDbContext = _seedScope.ServiceProvider.GetRequiredService<AppDbContext>();

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Clear existing data
        _seedDbContext.Database.EnsureDeleted();
        _seedDbContext.Database.EnsureCreated();

        // Bypass tenant filter to insert test data
        _seedDbContext.BypassTenantFilter = true;

        // Create organizations
        _org1 = new Organization { Name = "Org1", Code = "org1", CreatedAt = DateTimeOffset.UtcNow };
        _org2 = new Organization { Name = "Org2", Code = "org2", CreatedAt = DateTimeOffset.UtcNow };
        _seedDbContext.Organizations.AddRange(_org1, _org2);
        _seedDbContext.SaveChanges();

        // Create users
        _user1 = new User
        {
            Email = "user1@org1.com",
            Username = "user1",
            FirstName = "User",
            LastName = "One",
            GlobalRole = GlobalRole.User,
            OrganizationId = _org1.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _user2 = new User
        {
            Email = "user2@org2.com",
            Username = "user2",
            FirstName = "User",
            LastName = "Two",
            GlobalRole = GlobalRole.User,
            OrganizationId = _org2.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Users.AddRange(_user1, _user2);
        _seedDbContext.SaveChanges();

        // Create projects - 2 for org1, 1 for org2
        _project1 = new Project
        {
            Name = "Project1",
            Description = "Project 1 for Org1",
            OwnerId = _user1.Id,
            OrganizationId = _org1.Id,
            Status = ProjectConstants.Statuses.Active,
            Type = ProjectConstants.Types.Scrum,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var project1b = new Project
        {
            Name = "Project1B",
            Description = "Second project for Org1",
            OwnerId = _user1.Id,
            OrganizationId = _org1.Id,
            Status = ProjectConstants.Statuses.Active,
            Type = ProjectConstants.Types.Scrum,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _project2 = new Project
        {
            Name = "Project2",
            Description = "Project 2 for Org2",
            OwnerId = _user2.Id,
            OrganizationId = _org2.Id,
            Status = ProjectConstants.Statuses.Active,
            Type = ProjectConstants.Types.Scrum,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Projects.AddRange(_project1, project1b, _project2);
        _seedDbContext.SaveChanges();

        // Create completed sprints for velocity calculation
        _sprint1Completed = new Sprint
        {
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            Number = 1,
            Status = SprintConstants.Statuses.Completed,
            EndDate = DateTimeOffset.UtcNow.AddDays(-7),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-21)
        };
        var sprint1b = new Sprint
        {
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            Number = 2,
            Status = SprintConstants.Statuses.Active, // Active sprint
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-7)
        };
        _sprint2Completed = new Sprint
        {
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            Number = 1,
            Status = SprintConstants.Statuses.Completed,
            EndDate = DateTimeOffset.UtcNow.AddDays(-5),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-19)
        };
        _seedDbContext.Sprints.AddRange(_sprint1Completed, sprint1b, _sprint2Completed);
        _seedDbContext.SaveChanges();

        // Create tasks for org1 (3 tasks: 1 todo, 1 in progress, 1 done)
        _task1Todo = new ProjectTask
        {
            Title = "Task1 Todo",
            Description = "Todo task for Org1",
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            CreatedById = _user1.Id,
            Status = TaskConstants.Statuses.Todo,
            Priority = "Medium",
            StoryPoints = new StoryPoints(3),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _task1InProgress = new ProjectTask
        {
            Title = "Task1 InProgress",
            Description = "In progress task for Org1",
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            CreatedById = _user1.Id,
            Status = TaskConstants.Statuses.InProgress,
            Priority = "High",
            StoryPoints = new StoryPoints(5),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _task1Done = new ProjectTask
        {
            Title = "Task1 Done",
            Description = "Done task for Org1",
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            SprintId = _sprint1Completed.Id,
            CreatedById = _user1.Id,
            Status = TaskConstants.Statuses.Done,
            Priority = "Low",
            StoryPoints = new StoryPoints(8),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-14),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-7)
        };

        // Create tasks for org2 (2 tasks: 1 todo, 1 done)
        _task2Todo = new ProjectTask
        {
            Title = "Task2 Todo",
            Description = "Todo task for Org2",
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            CreatedById = _user2.Id,
            Status = TaskConstants.Statuses.Todo,
            Priority = "Medium",
            StoryPoints = new StoryPoints(2),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _task2Done = new ProjectTask
        {
            Title = "Task2 Done",
            Description = "Done task for Org2",
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            SprintId = _sprint2Completed.Id,
            CreatedById = _user2.Id,
            Status = TaskConstants.Statuses.Done,
            Priority = "High",
            StoryPoints = new StoryPoints(13),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
        };
        _seedDbContext.ProjectTasks.AddRange(_task1Todo, _task1InProgress, _task1Done, _task2Todo, _task2Done);
        _seedDbContext.SaveChanges();

        // Create defects
        _defect1Open = new Defect
        {
            Title = "Defect1 Open",
            Description = "Open defect for Org1",
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            ReportedById = _user1.Id,
            Severity = "High",
            Status = "Open",
            ReportedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var defect1Resolved = new Defect
        {
            Title = "Defect1 Resolved",
            Description = "Resolved defect for Org1",
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            ReportedById = _user1.Id,
            Severity = "Medium",
            Status = "Resolved",
            ReportedAt = DateTimeOffset.UtcNow.AddDays(-5),
            ResolvedAt = DateTimeOffset.UtcNow.AddDays(-2),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-2)
        };
        _defect2Open = new Defect
        {
            Title = "Defect2 Open",
            Description = "Open defect for Org2",
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            ReportedById = _user2.Id,
            Severity = "Critical",
            Status = "Open",
            ReportedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Defects.AddRange(_defect1Open, defect1Resolved, _defect2Open);
        _seedDbContext.SaveChanges();

        // Reset bypass flag
        _seedDbContext.BypassTenantFilter = false;
    }

    [Fact]
    public async TaskAsync GetMetricsSummary_Org1_ReturnsOnlyOrg1Data()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetMetricsSummaryQuery());

        // Assert - Org1 has 2 projects, 3 tasks, 1 open defect
        result.Should().NotBeNull();
        result.TotalProjects.Should().Be(2); // 2 projects in org1
        result.TotalTasks.Should().Be(3); // 3 tasks in org1
        result.OpenTasks.Should().Be(2); // 1 todo + 1 in progress
        result.CompletedTasks.Should().Be(1);
        result.DefectsCount.Should().Be(1); // 1 open defect
        result.TotalDefects.Should().Be(2); // 2 total defects (1 open + 1 resolved)
        result.ActiveSprints.Should().Be(1); // 1 active sprint
    }

    [Fact]
    public async TaskAsync GetMetricsSummary_Org2_ReturnsOnlyOrg2Data()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org2.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetMetricsSummaryQuery());

        // Assert - Org2 has 1 project, 2 tasks, 1 open defect
        result.Should().NotBeNull();
        result.TotalProjects.Should().Be(1); // 1 project in org2
        result.TotalTasks.Should().Be(2); // 2 tasks in org2
        result.OpenTasks.Should().Be(1); // 1 todo
        result.CompletedTasks.Should().Be(1);
        result.DefectsCount.Should().Be(1); // 1 open defect
        result.TotalDefects.Should().Be(1); // 1 total defect
        result.ActiveSprints.Should().Be(0); // No active sprints (only completed)
    }

    [Fact]
    public async TaskAsync GetSprintVelocityChart_Org1_ReturnsOnlyOrg1Sprints()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetSprintVelocityChartQuery());

        // Assert - Org1 has 1 completed sprint with 8 story points
        result.Should().NotBeNull();
        result.Sprints.Should().HaveCount(1);
        result.Sprints[0].Number.Should().Be(1);
        result.Sprints[0].StoryPoints.Should().Be(8); // task1Done has 8 story points
    }

    [Fact]
    public async TaskAsync GetSprintVelocityChart_Org2_ReturnsOnlyOrg2Sprints()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org2.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetSprintVelocityChartQuery());

        // Assert - Org2 has 1 completed sprint with 13 story points
        result.Should().NotBeNull();
        result.Sprints.Should().HaveCount(1);
        result.Sprints[0].Number.Should().Be(1);
        result.Sprints[0].StoryPoints.Should().Be(13); // task2Done has 13 story points
    }

    [Fact]
    public async TaskAsync GetTaskDistribution_Org1_ReturnsOnlyOrg1Tasks()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetTaskDistributionQuery());

        // Assert - Org1 has: 1 Todo, 1 InProgress, 0 Blocked, 1 Done
        result.Should().NotBeNull();
        result.Distribution.Should().NotBeEmpty();
        
        var todoCount = result.Distribution.FirstOrDefault(d => d.Status == "Todo")?.Count ?? 0;
        var inProgressCount = result.Distribution.FirstOrDefault(d => d.Status == "InProgress")?.Count ?? 0;
        var doneCount = result.Distribution.FirstOrDefault(d => d.Status == "Done")?.Count ?? 0;
        
        todoCount.Should().Be(1);
        inProgressCount.Should().Be(1);
        doneCount.Should().Be(1);
    }

    [Fact]
    public async TaskAsync GetTaskDistribution_Org2_ReturnsOnlyOrg2Tasks()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org2.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetTaskDistributionQuery());

        // Assert - Org2 has: 1 Todo, 0 InProgress, 0 Blocked, 1 Done
        result.Should().NotBeNull();
        result.Distribution.Should().NotBeEmpty();
        
        var todoCount = result.Distribution.FirstOrDefault(d => d.Status == "Todo")?.Count ?? 0;
        var inProgressCount = result.Distribution.FirstOrDefault(d => d.Status == "InProgress")?.Count ?? 0;
        var doneCount = result.Distribution.FirstOrDefault(d => d.Status == "Done")?.Count ?? 0;
        
        todoCount.Should().Be(1);
        inProgressCount.Should().Be(0);
        doneCount.Should().Be(1);
    }

    [Fact]
    public async TaskAsync GetMetricsSummary_BypassFilter_ReturnsAllData()
    {
        // Arrange - SuperAdmin can bypass filter
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = true; // SuperAdmin bypasses filter

        // Act
        var result = await mediator.Send(new GetMetricsSummaryQuery());

        // Assert - Should see all data from both organizations
        result.Should().NotBeNull();
        result.TotalProjects.Should().Be(3); // 2 from org1 + 1 from org2
        result.TotalTasks.Should().Be(5); // 3 from org1 + 2 from org2
        result.TotalDefects.Should().Be(3); // 2 from org1 + 1 from org2
    }

    [Fact]
    public async TaskAsync GetMetricsSummary_ProjectFilter_ReturnsOnlyProjectData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act - Filter by specific project
        var result = await mediator.Send(new GetMetricsSummaryQuery { ProjectId = _project1.Id });

        // Assert - Should only count tasks from project1 (all 3 tasks are in project1)
        result.Should().NotBeNull();
        result.TotalTasks.Should().Be(3);
        result.TotalProjects.Should().Be(1); // Only counting the filtered project
    }

    [Fact]
    public async TaskAsync GetMetricsSummary_VelocityCalculation_UsesCompletedSprints()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetMetricsSummaryQuery());

        // Assert - Velocity should be calculated from completed sprints
        // Org1 has 1 completed sprint with 8 story points, so average velocity = 8
        result.Should().NotBeNull();
        result.Velocity.Should().Be(8);
    }

    public void Dispose()
    {
        _seedScope?.Dispose();
    }
}
