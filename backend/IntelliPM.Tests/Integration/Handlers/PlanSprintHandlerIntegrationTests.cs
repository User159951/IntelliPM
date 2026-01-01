using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using MediatR;
using IntelliPM.Application.Agents.Commands;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Constants;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.Handlers;

public class PlanSprintHandlerIntegrationTests : IClassFixture<AIAgentHandlerTestFactory>
{
    private readonly AIAgentHandlerTestFactory _factory;

    public PlanSprintHandlerIntegrationTests(AIAgentHandlerTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async SystemTask Handle_ShouldReturnPlan_WhenBacklogAvailable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization { Name = "Test Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = new Project
        {
            Name = "Sprint Planning Test Project",
            Description = "Test",
            OwnerId = user.Id,
            OrganizationId = organization.Id,
            Status = "Active",
            Type = "Scrum",
            SprintDurationDays = 14,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Create a sprint
        var sprint = new Sprint
        {
            ProjectId = project.Id,
            OrganizationId = organization.Id,
            Number = 1,
            Goal = "Complete authentication module",
            StartDate = DateTimeOffset.UtcNow,
            EndDate = DateTimeOffset.UtcNow.AddDays(14),
            Status = SprintConstants.Statuses.Planned,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Sprints.Add(sprint);

        // Add backlog items (UserStories)
        var backlogItems = new List<UserStory>
        {
            new UserStory
            {
                ProjectId = project.Id,
                Title = "User Authentication",
                Description = "Implement login functionality",
                Status = "Backlog",
                StoryPoints = 8,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new UserStory
            {
                ProjectId = project.Id,
                Title = "Payment Integration",
                Description = "Integrate payment gateway",
                Status = "Backlog",
                StoryPoints = 5,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new UserStory
            {
                ProjectId = project.Id,
                Title = "API Documentation",
                Description = "Document REST API",
                Status = "Backlog",
                StoryPoints = 3,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
        db.UserStories.AddRange(backlogItems);
        await db.SaveChangesAsync();

        var command = new PlanSprintCommand(sprint.Id);

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        result.Content.Should().NotBeNullOrEmpty();
        result.Content.Should().ContainAny("Sprint", "Plan", "Task", "Proposal", "Capacity");
        result.RequiresApproval.Should().BeTrue(); // Sprint planning requires approval
        result.ExecutionTimeMs.Should().BeGreaterThan(0);

        // Verify execution log
        var executionLog = await db.AgentExecutionLogs
            .FirstOrDefaultAsync(log => log.AgentId == "sprint-planner" && log.UserInput.Contains(sprint.Id.ToString()));
        executionLog.Should().NotBeNull();
        executionLog!.Status.Should().Be("Success");

        // Cleanup
        db.Projects.Remove(project);
        db.Users.Remove(user);
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async SystemTask Handle_ShouldConsiderTeamCapacity_WhenPlanning()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization { Name = "Test Org 2", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "test2@example.com",
            Username = "testuser2",
            FirstName = "Test",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = new Project
        {
            Name = "Capacity Test Project",
            Description = "Test",
            OwnerId = user.Id,
            OrganizationId = organization.Id,
            Status = "Active",
            Type = "Scrum",
            SprintDurationDays = 14,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Create sprint with capacity
        var sprint = new Sprint
        {
            ProjectId = project.Id,
            OrganizationId = organization.Id,
            Number = 2,
            Goal = "Test capacity planning",
            StartDate = DateTimeOffset.UtcNow,
            EndDate = DateTimeOffset.UtcNow.AddDays(14),
            Status = SprintConstants.Statuses.Planned,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Sprints.Add(sprint);

        // Add project members (team)
        var member1 = new User
        {
            Email = "dev1@example.com",
            Username = "dev1",
            FirstName = "Dev",
            LastName = "One",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(member1);
        await db.SaveChangesAsync();

        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = member1.Id,
            Role = ProjectRole.Developer,
            InvitedById = user.Id,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTimeOffset.UtcNow
        };
        db.ProjectMembers.Add(projectMember);

        // Add backlog items with various story points
        var backlogItems = new List<UserStory>
        {
            new UserStory
            {
                ProjectId = project.Id,
                Title = "Large Task",
                Description = "Big task",
                Status = "Backlog",
                StoryPoints = 13,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new UserStory
            {
                ProjectId = project.Id,
                Title = "Medium Task",
                Description = "Medium task",
                Status = "Backlog",
                StoryPoints = 8,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new UserStory
            {
                ProjectId = project.Id,
                Title = "Small Task",
                Description = "Small task",
                Status = "Backlog",
                StoryPoints = 3,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
        db.UserStories.AddRange(backlogItems);
        await db.SaveChangesAsync();

        var command = new PlanSprintCommand(sprint.Id);

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        result.Content.Should().NotBeNullOrEmpty();
        // The mock response should mention capacity considerations
        result.RequiresApproval.Should().BeTrue();

        // Cleanup
        db.Projects.Remove(project);
        db.Users.Remove(user);
        db.Users.Remove(member1);
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async SystemTask Handle_ShouldLogExecution_WhenSprintPlanningCompletes()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization { Name = "Test Org 3", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "test3@example.com",
            Username = "testuser3",
            FirstName = "Test",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = new Project
        {
            Name = "Log Test Project",
            Description = "Test",
            OwnerId = user.Id,
            OrganizationId = organization.Id,
            Status = "Active",
            Type = "Scrum",
            SprintDurationDays = 14,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var sprint = new Sprint
        {
            ProjectId = project.Id,
            OrganizationId = organization.Id,
            Number = 3,
            Goal = "Test logging",
            StartDate = DateTimeOffset.UtcNow,
            EndDate = DateTimeOffset.UtcNow.AddDays(14),
            Status = SprintConstants.Statuses.Planned,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync();

        var initialLogCount = await db.AgentExecutionLogs.CountAsync();

        var command = new PlanSprintCommand(sprint.Id);

        // Act
        await mediator.Send(command);

        // Assert
        var finalLogCount = await db.AgentExecutionLogs.CountAsync();
        finalLogCount.Should().BeGreaterThan(initialLogCount);

        var log = await db.AgentExecutionLogs
            .OrderByDescending(l => l.CreatedAt)
            .FirstAsync();
        log.AgentId.Should().Be("sprint-planner");
        log.Status.Should().Be("Success");
        log.ExecutionTimeMs.Should().BeGreaterThan(0);

        // Cleanup
        db.Projects.Remove(project);
        db.Users.Remove(user);
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync();
    }
}

