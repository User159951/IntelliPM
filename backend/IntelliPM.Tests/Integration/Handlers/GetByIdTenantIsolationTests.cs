using FluentAssertions;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Defects.Queries;
using IntelliPM.Application.Features.Milestones.Queries;
using IntelliPM.Application.Features.Releases.Queries;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Application.Sprints.Queries;
using IntelliPM.Application.Tasks.Queries;
using IntelliPM.Application.Teams.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskAsync = System.Threading.Tasks.Task;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntelliPM.Tests.Integration.Handlers;

/// <summary>
/// Integration tests to verify tenant isolation in GetById query handlers.
/// Ensures that accessing entities from another organization returns 404 (not the data).
/// </summary>
public class GetByIdTenantIsolationTests : IClassFixture<AIAgentHandlerTestFactory>, IDisposable
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
    private ProjectTask _task1 = null!;
    private ProjectTask _task2 = null!;
    private Sprint _sprint1 = null!;
    private Sprint _sprint2 = null!;
    private Defect _defect1 = null!;
    private Defect _defect2 = null!;
    private Milestone _milestone1 = null!;
    private Milestone _milestone2 = null!;
    private Release _release1 = null!;
    private Release _release2 = null!;
    private Team _team1 = null!;
    private Team _team2 = null!;

    public GetByIdTenantIsolationTests(AIAgentHandlerTestFactory factory)
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
        _org1 = new Organization { Name = "Org1", CreatedAt = DateTimeOffset.UtcNow };
        _org2 = new Organization { Name = "Org2", CreatedAt = DateTimeOffset.UtcNow };
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

        // Create projects
        _project1 = new Project
        {
            Name = "Project1",
            Description = "Project 1",
            OwnerId = _user1.Id,
            OrganizationId = _org1.Id,
            Status = "Active",
            Type = "Scrum",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _project2 = new Project
        {
            Name = "Project2",
            Description = "Project 2",
            OwnerId = _user2.Id,
            OrganizationId = _org2.Id,
            Status = "Active",
            Type = "Scrum",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Projects.AddRange(_project1, _project2);
        _seedDbContext.SaveChanges();

        // Create tasks
        _task1 = new ProjectTask
        {
            Title = "Task1",
            Description = "Task 1",
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            CreatedById = _user1.Id,
            Status = "ToDo",
            Priority = "Medium",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _task2 = new ProjectTask
        {
            Title = "Task2",
            Description = "Task 2",
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            CreatedById = _user2.Id,
            Status = "ToDo",
            Priority = "Medium",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.ProjectTasks.AddRange(_task1, _task2);
        _seedDbContext.SaveChanges();

        // Create sprints
        _sprint1 = new Sprint
        {
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            Number = 1,
            Status = "Planned",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _sprint2 = new Sprint
        {
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            Number = 1,
            Status = "Planned",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Sprints.AddRange(_sprint1, _sprint2);
        _seedDbContext.SaveChanges();

        // Create defects
        _defect1 = new Defect
        {
            Title = "Defect1",
            Description = "Defect 1",
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            ReportedById = _user1.Id,
            Severity = "High",
            Status = "Open",
            ReportedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _defect2 = new Defect
        {
            Title = "Defect2",
            Description = "Defect 2",
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            ReportedById = _user2.Id,
            Severity = "High",
            Status = "Open",
            ReportedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Defects.AddRange(_defect1, _defect2);
        _seedDbContext.SaveChanges();

        // Create milestones
        _milestone1 = new Milestone
        {
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            Name = "Milestone1",
            Description = "Milestone 1",
            Type = MilestoneType.Release,
            Status = MilestoneStatus.Pending,
            DueDate = DateTimeOffset.UtcNow.AddDays(30),
            CreatedById = _user1.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _milestone2 = new Milestone
        {
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            Name = "Milestone2",
            Description = "Milestone 2",
            Type = MilestoneType.Release,
            Status = MilestoneStatus.Pending,
            DueDate = DateTimeOffset.UtcNow.AddDays(30),
            CreatedById = _user2.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Milestones.AddRange(_milestone1, _milestone2);
        _seedDbContext.SaveChanges();

        // Create releases
        _release1 = new Release
        {
            ProjectId = _project1.Id,
            OrganizationId = _org1.Id,
            Name = "Release1",
            Version = "1.0.0",
            Type = ReleaseType.Major,
            Status = ReleaseStatus.Planned,
            CreatedById = _user1.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _release2 = new Release
        {
            ProjectId = _project2.Id,
            OrganizationId = _org2.Id,
            Name = "Release2",
            Version = "1.0.0",
            Type = ReleaseType.Major,
            Status = ReleaseStatus.Planned,
            CreatedById = _user2.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Releases.AddRange(_release1, _release2);
        _seedDbContext.SaveChanges();

        // Create teams
        _team1 = new Team
        {
            Name = "Team1",
            OrganizationId = _org1.Id,
            Capacity = 10,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _team2 = new Team
        {
            Name = "Team2",
            OrganizationId = _org2.Id,
            Capacity = 10,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Teams.AddRange(_team1, _team2);
        _seedDbContext.SaveChanges();

        // Reset bypass flag
        _seedDbContext.BypassTenantFilter = false;
    }

    [Fact]
    public async TaskAsync GetProjectById_CrossTenantAccess_Returns404()
    {
        // Arrange - Create a new scope for this test and set tenant context to org1
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act & Assert - Try to access org2's project
        var act = async () => await mediator.Send(new GetProjectByIdQuery(_project2.Id));
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Project not found");
    }

    [Fact]
    public async TaskAsync GetProjectById_SameTenant_ReturnsProject()
    {
        // Arrange - Create a new scope for this test and set tenant context to org1
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetProjectByIdQuery(_project1.Id));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_project1.Id);
        result.Name.Should().Be("Project1");
    }

    [Fact]
    public async TaskAsync GetTaskById_CrossTenantAccess_Returns404()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act & Assert
        var act = async () => await mediator.Send(new GetTaskByIdQuery(_task2.Id));
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Task not found");
    }

    [Fact]
    public async TaskAsync GetTaskById_SameTenant_ReturnsTask()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetTaskByIdQuery(_task1.Id));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_task1.Id);
        result.Title.Should().Be("Task1");
    }

    [Fact]
    public async TaskAsync GetSprintById_CrossTenantAccess_Returns404()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act & Assert
        var act = async () => await mediator.Send(new GetSprintByIdQuery(_sprint2.Id));
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Sprint not found");
    }

    [Fact]
    public async TaskAsync GetSprintById_SameTenant_ReturnsSprint()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetSprintByIdQuery(_sprint1.Id));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_sprint1.Id);
    }

    [Fact]
    public async TaskAsync GetDefectById_CrossTenantAccess_Returns404()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act & Assert
        var act = async () => await mediator.Send(new GetDefectByIdQuery(_defect2.Id));
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Defect not found");
    }

    [Fact]
    public async TaskAsync GetDefectById_SameTenant_ReturnsDefect()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetDefectByIdQuery(_defect1.Id));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_defect1.Id);
        result.Title.Should().Be("Defect1");
    }

    [Fact]
    public async TaskAsync GetMilestoneById_CrossTenantAccess_Returns404()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act & Assert
        var act = async () => await mediator.Send(new GetMilestoneByIdQuery(_milestone2.Id));
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Milestone not found");
    }

    [Fact]
    public async TaskAsync GetMilestoneById_SameTenant_ReturnsMilestone()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetMilestoneByIdQuery(_milestone1.Id));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_milestone1.Id);
        result.Name.Should().Be("Milestone1");
    }

    [Fact]
    public async TaskAsync GetReleaseById_CrossTenantAccess_Returns404()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act & Assert
        var act = async () => await mediator.Send(new GetReleaseByIdQuery(_release2.Id));
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Release not found");
    }

    [Fact]
    public async TaskAsync GetReleaseById_SameTenant_ReturnsRelease()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetReleaseByIdQuery(_release1.Id));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_release1.Id);
        result.Name.Should().Be("Release1");
    }

    [Fact]
    public async TaskAsync GetTeamById_CrossTenantAccess_Returns404()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act & Assert
        var act = async () => await mediator.Send(new GetTeamByIdQuery(_team2.Id));
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Team not found");
    }

    [Fact]
    public async TaskAsync GetTeamById_SameTenant_ReturnsTeam()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var result = await mediator.Send(new GetTeamByIdQuery(_team1.Id));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_team1.Id);
        result.Name.Should().Be("Team1");
    }

    public void Dispose()
    {
        _seedScope?.Dispose();
    }
}
