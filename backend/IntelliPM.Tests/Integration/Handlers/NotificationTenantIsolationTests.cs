using FluentAssertions;
using IntelliPM.Application.Notifications.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using TaskAsync = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Integration.Handlers;

/// <summary>
/// Integration tests to verify tenant isolation in notification queries.
/// Ensures that users only see notifications from their own organization.
/// </summary>
public class NotificationTenantIsolationTests : IClassFixture<AIAgentHandlerTestFactory>, IDisposable
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
    private Notification _notification1 = null!; // For user1 in org1
    private Notification _notification2 = null!; // For user2 in org2
    private Notification _notification3 = null!; // For user1 in org1 (different notification)

    public NotificationTenantIsolationTests(AIAgentHandlerTestFactory factory)
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

        // Create notifications
        _notification1 = new Notification
        {
            UserId = _user1.Id,
            OrganizationId = _org1.Id,
            Type = "task_assigned",
            Message = "Task assigned to user1 in org1",
            EntityType = "task",
            EntityId = 1,
            ProjectId = _project1.Id,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        };
        _notification2 = new Notification
        {
            UserId = _user2.Id,
            OrganizationId = _org2.Id,
            Type = "task_assigned",
            Message = "Task assigned to user2 in org2",
            EntityType = "task",
            EntityId = 2,
            ProjectId = _project2.Id,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        _notification3 = new Notification
        {
            UserId = _user1.Id,
            OrganizationId = _org1.Id,
            Type = "task_completed",
            Message = "Task completed for user1 in org1",
            EntityType = "task",
            EntityId = 3,
            ProjectId = _project1.Id,
            IsRead = true,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        _seedDbContext.Notifications.AddRange(_notification1, _notification2, _notification3);
        _seedDbContext.SaveChanges();

        // Reset bypass flag
        _seedDbContext.BypassTenantFilter = false;
    }

    [Fact]
    public async TaskAsync GetNotifications_CrossTenantAccess_ReturnsOnlyOwnNotifications()
    {
        // Arrange - Create a new scope for this test and set tenant context to org1
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act - Query notifications for user1 in org1
        var query = new GetNotificationsQuery
        {
            UserId = _user1.Id,
            OrganizationId = _org1.Id,
            UnreadOnly = false,
            Limit = 10,
            Offset = 0
        };
        var result = await mediator.Send(query);

        // Assert - Should only see notifications for user1 in org1
        result.Should().NotBeNull();
        result.Notifications.Should().HaveCount(2); // notification1 and notification3
        result.Notifications.Should().OnlyContain(n => n.Id == _notification1.Id || n.Id == _notification3.Id);
        result.Notifications.Should().NotContain(n => n.Id == _notification2.Id); // Should not see org2's notification
    }

    [Fact]
    public async TaskAsync GetNotifications_CrossTenantUser_ReturnsEmpty()
    {
        // Arrange - Query as user1 but with org2's context (should return empty)
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org2.Id;
        dbContext.BypassTenantFilter = false;

        // Act - Query notifications for user1 but with org2's organizationId
        var query = new GetNotificationsQuery
        {
            UserId = _user1.Id,
            OrganizationId = _org2.Id, // Wrong organization
            UnreadOnly = false,
            Limit = 10,
            Offset = 0
        };
        var result = await mediator.Send(query);

        // Assert - Should return empty (user1 doesn't have notifications in org2)
        result.Should().NotBeNull();
        result.Notifications.Should().BeEmpty();
        result.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async TaskAsync GetNotifications_UnreadOnly_FiltersCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act - Query unread notifications only for user1 in org1
        var query = new GetNotificationsQuery
        {
            UserId = _user1.Id,
            OrganizationId = _org1.Id,
            UnreadOnly = true,
            Limit = 10,
            Offset = 0
        };
        var result = await mediator.Send(query);

        // Assert - Should only return unread notifications
        result.Should().NotBeNull();
        result.Notifications.Should().HaveCount(1); // Only notification1 (unread)
        result.Notifications.Should().OnlyContain(n => n.Id == _notification1.Id);
        result.Notifications.Should().NotContain(n => n.Id == _notification3.Id); // notification3 is read
        result.UnreadCount.Should().Be(1);
    }

    [Fact]
    public async TaskAsync GetNotifications_Pagination_WorksCorrectly()
    {
        // Arrange - Create more notifications for user1
        _seedDbContext.BypassTenantFilter = true;
        for (int i = 0; i < 5; i++)
        {
            var notification = new Notification
            {
                UserId = _user1.Id,
                OrganizationId = _org1.Id,
                Type = "task_assigned",
                Message = $"Notification {i}",
                EntityType = "task",
                EntityId = 100 + i,
                ProjectId = _project1.Id,
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-i)
            };
            _seedDbContext.Notifications.Add(notification);
        }
        _seedDbContext.SaveChanges();
        _seedDbContext.BypassTenantFilter = false;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act - Query first page (limit 3)
        var query1 = new GetNotificationsQuery
        {
            UserId = _user1.Id,
            OrganizationId = _org1.Id,
            UnreadOnly = false,
            Limit = 3,
            Offset = 0
        };
        var result1 = await mediator.Send(query1);

        // Act - Query second page (limit 3, offset 3)
        var query2 = new GetNotificationsQuery
        {
            UserId = _user1.Id,
            OrganizationId = _org1.Id,
            UnreadOnly = false,
            Limit = 3,
            Offset = 3
        };
        var result2 = await mediator.Send(query2);

        // Assert - First page should have 3 notifications
        result1.Should().NotBeNull();
        result1.Notifications.Should().HaveCount(3);

        // Assert - Second page should have remaining notifications
        result2.Should().NotBeNull();
        result2.Notifications.Should().HaveCountGreaterThan(0);
        result2.Notifications.Should().HaveCountLessThanOrEqualTo(3);

        // Assert - No overlap between pages
        var page1Ids = result1.Notifications.Select(n => n.Id).ToHashSet();
        var page2Ids = result2.Notifications.Select(n => n.Id).ToHashSet();
        page1Ids.Should().NotIntersectWith(page2Ids);
    }

    [Fact]
    public async TaskAsync GetNotifications_Ordering_IsDescendingByCreatedAt()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        dbContext.CurrentOrganizationId = _org1.Id;
        dbContext.BypassTenantFilter = false;

        // Act
        var query = new GetNotificationsQuery
        {
            UserId = _user1.Id,
            OrganizationId = _org1.Id,
            UnreadOnly = false,
            Limit = 10,
            Offset = 0
        };
        var result = await mediator.Send(query);

        // Assert - Should be ordered by CreatedAt descending (most recent first)
        result.Should().NotBeNull();
        result.Notifications.Should().HaveCountGreaterThan(1);
        
        var notifications = result.Notifications.ToList();
        for (int i = 0; i < notifications.Count - 1; i++)
        {
            notifications[i].CreatedAt.Should().BeOnOrAfter(notifications[i + 1].CreatedAt);
        }
    }

    public void Dispose()
    {
        _seedScope?.Dispose();
    }
}
