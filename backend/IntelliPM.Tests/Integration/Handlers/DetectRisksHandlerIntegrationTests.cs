using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using MediatR;
using IntelliPM.Application.Agents.Commands;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.Handlers;

public class DetectRisksHandlerIntegrationTests : IClassFixture<AIAgentHandlerTestFactory>
{
    private readonly AIAgentHandlerTestFactory _factory;

    public DetectRisksHandlerIntegrationTests(AIAgentHandlerTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async SystemTask Handle_ShouldReturnRisks_WhenRisksExist()
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
            Name = "Risk Test Project",
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

        // Add some blocked tasks (risks)
        var blockedTasks = new List<ProjectTask>
        {
            new ProjectTask
            {
                ProjectId = project.Id,
                Title = "Blocked Task 1",
                Description = "Blocked by external dependency",
                Status = "Blocked",
                Priority = "High",
                CreatedById = user.Id,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new ProjectTask
            {
                ProjectId = project.Id,
                Title = "Blocked Task 2",
                Description = "Blocked by resource unavailability",
                Status = "Blocked",
                Priority = "Critical",
                CreatedById = user.Id,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
        db.ProjectTasks.AddRange(blockedTasks);

        // Add some risks
        var risks = new List<Risk>
        {
            new Risk
            {
                ProjectId = project.Id,
                Title = "External API Dependency",
                Description = "Risk of delay",
                Status = "Open",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
        db.Risks.AddRange(risks);
        await db.SaveChangesAsync();

        var command = new DetectRisksCommand(project.Id);

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        result.Content.Should().NotBeNullOrEmpty();
        result.Content.Should().ContainAny("Risk", "Analysis", "High-Priority", "Recommendation");
        result.ExecutionTimeMs.Should().BeGreaterThan(0);

        // Verify execution log
        var executionLog = await db.AgentExecutionLogs
            .FirstOrDefaultAsync(log => log.AgentId == "risk-detector" && log.UserInput.Contains(project.Id.ToString()));
        executionLog.Should().NotBeNull();
        executionLog!.Status.Should().Be("Success");

        // Verify alert was created
        var alert = await db.Alerts
            .FirstOrDefaultAsync(a => a.ProjectId == project.Id && a.Type == "RiskAnalysis");
        alert.Should().NotBeNull();

        // Cleanup
        db.Projects.Remove(project);
        db.Users.Remove(user);
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async SystemTask Handle_ShouldReturnAnalysis_WhenNoRisks()
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
            Name = "No Risk Project",
            Description = "Test",
            OwnerId = user.Id,
            OrganizationId = organization.Id,
            Status = "Active",
            Type = "Scrum",
            SprintDurationDays = 14,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Projects.Add(project);
        
        // Add only completed tasks (no risks)
        var completedTask = new ProjectTask
        {
            ProjectId = project.Id,
            Title = "Completed Task",
            Description = "Done",
            Status = "Done",
            Priority = "Low",
            CreatedById = user.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.ProjectTasks.Add(completedTask);
        await db.SaveChangesAsync();

        var command = new DetectRisksCommand(project.Id);

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        result.Content.Should().NotBeNullOrEmpty();

        // Cleanup
        db.Projects.Remove(project);
        db.Users.Remove(user);
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async SystemTask Handle_ShouldCreateAlert_WhenRiskAnalysisCompletes()
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
            Name = "Alert Test Project",
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

        var initialAlertCount = await db.Alerts.CountAsync(a => a.ProjectId == project.Id);

        var command = new DetectRisksCommand(project.Id);

        // Act
        await mediator.Send(command);

        // Assert
        var finalAlertCount = await db.Alerts.CountAsync(a => a.ProjectId == project.Id);
        finalAlertCount.Should().Be(initialAlertCount + 1);

        var alert = await db.Alerts
            .FirstOrDefaultAsync(a => a.ProjectId == project.Id && a.Type == "RiskAnalysis");
        alert.Should().NotBeNull();
        alert!.Type.Should().Be("RiskAnalysis");
        alert.Severity.Should().Be("Warning");

        // Cleanup
        db.Projects.Remove(project);
        db.Users.Remove(user);
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync();
    }
}

