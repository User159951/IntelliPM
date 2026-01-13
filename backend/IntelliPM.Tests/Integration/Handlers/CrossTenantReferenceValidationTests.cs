using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Projects.Commands;
using IntelliPM.Application.Tasks.Commands;
using IntelliPM.Application.Defects.Commands;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Tests.API;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.Handlers;

/// <summary>
/// Integration tests to verify that command handlers prevent cross-tenant references.
/// Tests that users cannot assign/reference users from other organizations.
/// </summary>
public class CrossTenantReferenceValidationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;

    // Test data IDs
    private const int Org1Id = 1;
    private const int Org2Id = 2;
    private const int User1Org1Id = 1;
    private const int User2Org1Id = 2;
    private const int User1Org2Id = 3;
    private const int Project1Org1Id = 1;

    public CrossTenantReferenceValidationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Create mock ICurrentUserService
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(s => s.GetUserId()).Returns(User1Org1Id);
        _mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(Org1Id);
        _mockCurrentUserService.Setup(s => s.GetGlobalRole()).Returns(GlobalRole.User);
        _mockCurrentUserService.Setup(s => s.IsAdmin()).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsSuperAdmin()).Returns(false);
        
        // Get mediator from the existing scope
        // Note: ICurrentUserService will be resolved from the real implementation
        // The handlers will use the real ICurrentUserService, not the mock
        // This is a limitation of the current test setup
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
        
        // Seed test data
        SeedTestDataAsync().GetAwaiter().GetResult();
    }

    private async Task SeedTestDataAsync()
    {
        // Clear existing data
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Create organizations
        var org1 = new Organization { Id = Org1Id, Name = "Organization 1", Code = "org1", CreatedAt = DateTimeOffset.UtcNow };
        var org2 = new Organization { Id = Org2Id, Name = "Organization 2", Code = "org2", CreatedAt = DateTimeOffset.UtcNow };
        _context.Organizations.AddRange(org1, org2);

        // Create users for Organization 1
        var user1Org1 = new User
        {
            Id = User1Org1Id,
            Username = "user1org1",
            Email = "user1@org1.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = Org1Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        var user2Org1 = new User
        {
            Id = User2Org1Id,
            Username = "user2org1",
            Email = "user2@org1.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = Org1Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        // Create user for Organization 2
        var user1Org2 = new User
        {
            Id = User1Org2Id,
            Username = "user1org2",
            Email = "user1@org2.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = Org2Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        _context.Users.AddRange(user1Org1, user2Org1, user1Org2);

        // Create project for Organization 1
        var project1Org1 = new Project
        {
            Id = Project1Org1Id,
            Name = "Project 1 Org1",
            Description = "Test Project",
            Type = "Scrum",
            SprintDurationDays = 14,
            OwnerId = User1Org1Id,
            OrganizationId = Org1Id,
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Projects.Add(project1Org1);

        // Add owner as project member
        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = Project1Org1Id,
            UserId = User1Org1Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = User1Org1Id,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateProject_WithCrossOrgMember_ShouldThrowValidationException()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(Org1Id);
        
        var command = new CreateProjectCommand(
            Name: "New Project",
            Description: "Test Project",
            Type: "Scrum",
            SprintDurationDays: 14,
            OwnerId: User1Org1Id,
            Status: "Active",
            StartDate: null,
            MemberIds: new List<int> { User1Org2Id } // User from different organization
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _mediator.Send(command));

        exception.Message.Should().Contain("do not belong to your organization");
    }

    [Fact]
    public async Task CreateProject_WithValidMembers_ShouldSucceed()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(Org1Id);
        
        var command = new CreateProjectCommand(
            Name: "New Project",
            Description: "Test Project",
            Type: "Scrum",
            SprintDurationDays: 14,
            OwnerId: User1Org1Id,
            Status: "Active",
            StartDate: null,
            MemberIds: new List<int> { User2Org1Id } // User from same organization
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateTask_WithCrossOrgAssignee_ShouldThrowValidationException()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(Org1Id);
        
        var command = new CreateTaskCommand(
            Title: "Test Task",
            Description: "Test Description",
            ProjectId: Project1Org1Id,
            Priority: "High",
            StoryPoints: 5,
            AssigneeId: User1Org2Id, // User from different organization
            CreatedById: User1Org1Id
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _mediator.Send(command));

        exception.Message.Should().Contain("does not belong to your organization");
    }

    [Fact]
    public async Task CreateTask_WithValidAssignee_ShouldSucceed()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(Org1Id);
        
        var command = new CreateTaskCommand(
            Title: "Test Task",
            Description: "Test Description",
            ProjectId: Project1Org1Id,
            Priority: "High",
            StoryPoints: 5,
            AssigneeId: User2Org1Id, // User from same organization
            CreatedById: User1Org1Id
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AssignTask_WithCrossOrgAssignee_ShouldThrowValidationException()
    {
        // Arrange - First create a task
        _mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(Org1Id);
        
        var createTaskCommand = new CreateTaskCommand(
            Title: "Test Task",
            Description: "Test Description",
            ProjectId: Project1Org1Id,
            Priority: "High",
            StoryPoints: 5,
            AssigneeId: null,
            CreatedById: User1Org1Id
        );

        var task = await _mediator.Send(createTaskCommand);

        // Now try to assign to cross-org user
        var assignCommand = new AssignTaskCommand(
            TaskId: task.Id,
            AssigneeId: User1Org2Id, // User from different organization
            UpdatedBy: User1Org1Id
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _mediator.Send(assignCommand));

        exception.Message.Should().Contain("does not belong to your organization");
    }

    [Fact]
    public async Task CreateDefect_WithCrossOrgAssignee_ShouldThrowValidationException()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(Org1Id);
        
        var command = new CreateDefectCommand(
            ProjectId: Project1Org1Id,
            UserStoryId: null,
            SprintId: null,
            Title: "Test Defect",
            Description: "Test Description",
            Severity: "High",
            ReportedById: User1Org1Id,
            AssignedToId: User1Org2Id, // User from different organization
            FoundInEnvironment: "Test",
            StepsToReproduce: "Steps"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _mediator.Send(command));

        exception.Message.Should().Contain("does not belong to your organization");
    }

    [Fact]
    public async Task InviteMember_WithCrossOrgUser_ShouldThrowValidationException()
    {
        // Arrange
        _mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(Org1Id);
        
        var command = new InviteMemberCommand(
            ProjectId: Project1Org1Id,
            Email: "user1@org2.com", // Email of user from different organization
            Role: ProjectRole.Developer,
            CurrentUserId: User1Org1Id
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _mediator.Send(command));

        exception.Message.Should().Contain("does not belong to your organization");
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}
