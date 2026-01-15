using FluentAssertions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Search.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using TaskAsync = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Integration.Handlers;

/// <summary>
/// Integration tests to verify tenant isolation in SearchQueryHandler.
/// Ensures that search results are strictly filtered by organization and never expose cross-tenant data.
/// </summary>
public class SearchQueryHandlerTenantIsolationTests : IClassFixture<AIAgentHandlerTestFactory>, IDisposable
{
    private readonly AIAgentHandlerTestFactory _factory;
    private readonly IServiceScope _seedScope;
    private readonly AppDbContext _seedDbContext;

    private Organization _org1 = null!;
    private Organization _org2 = null!;
    private User _user1Org1 = null!;
    private User _user2Org1 = null!;
    private User _user1Org2 = null!;
    private Project _project1Org1 = null!;
    private Project _project2Org2 = null!;
    private ProjectTask _task1Org1 = null!;
    private ProjectTask _task2Org2 = null!;
    private Comment _comment1Org1 = null!;
    private Comment _comment2Org2 = null!;

    public SearchQueryHandlerTenantIsolationTests(AIAgentHandlerTestFactory factory)
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
        _user1Org1 = new User
        {
            Email = "user1@org1.com",
            Username = "user1org1",
            FirstName = "User",
            LastName = "One",
            GlobalRole = GlobalRole.User,
            OrganizationId = _org1.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _user2Org1 = new User
        {
            Email = "user2@org1.com",
            Username = "user2org1",
            FirstName = "User",
            LastName = "Two",
            GlobalRole = GlobalRole.User,
            OrganizationId = _org1.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _user1Org2 = new User
        {
            Email = "user1@org2.com",
            Username = "user1org2",
            FirstName = "User",
            LastName = "Three",
            GlobalRole = GlobalRole.User,
            OrganizationId = _org2.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Users.AddRange(_user1Org1, _user2Org1, _user1Org2);
        _seedDbContext.SaveChanges();

        // Create projects
        _project1Org1 = new Project
        {
            Name = "Project Alpha",
            Description = "Alpha project description",
            Type = "Scrum",
            SprintDurationDays = 14,
            OwnerId = _user1Org1.Id,
            OrganizationId = _org1.Id,
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _project2Org2 = new Project
        {
            Name = "Project Beta",
            Description = "Beta project description",
            Type = "Kanban",
            SprintDurationDays = 7,
            OwnerId = _user1Org2.Id,
            OrganizationId = _org2.Id,
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Projects.AddRange(_project1Org1, _project2Org2);
        _seedDbContext.SaveChanges();

        // Create tasks
        _task1Org1 = new ProjectTask
        {
            ProjectId = _project1Org1.Id,
            OrganizationId = _org1.Id,
            Title = "Task Alpha",
            Description = "Alpha task description",
            Status = "Todo",
            Priority = "High",
            CreatedById = _user1Org1.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _task2Org2 = new ProjectTask
        {
            ProjectId = _project2Org2.Id,
            OrganizationId = _org2.Id,
            Title = "Task Beta",
            Description = "Beta task description",
            Status = "InProgress",
            Priority = "Medium",
            CreatedById = _user1Org2.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.ProjectTasks.AddRange(_task1Org1, _task2Org2);
        _seedDbContext.SaveChanges();

        // Create comments
        _comment1Org1 = new Comment
        {
            OrganizationId = _org1.Id,
            EntityType = "Task",
            EntityId = _task1Org1.Id,
            Content = "This is a comment on Alpha task",
            AuthorId = _user1Org1.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };
        _comment2Org2 = new Comment
        {
            OrganizationId = _org2.Id,
            EntityType = "Task",
            EntityId = _task2Org2.Id,
            Content = "This is a comment on Beta task",
            AuthorId = _user1Org2.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };
        _seedDbContext.Comments.AddRange(_comment1Org1, _comment2Org2);
        _seedDbContext.SaveChanges();

        // Reset bypass flag
        _seedDbContext.BypassTenantFilter = false;
    }

    [Fact]
    public async TaskAsync SearchProjects_UserFromOrg1_OnlyReturnsOrg1Projects()
    {
        // Arrange - Create scope with mocked ICurrentUserService for Org1
        using var scope = _factory.Services.CreateScope();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_user1Org1.Id);
        mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(_org1.Id);
        mockCurrentUserService.Setup(s => s.GetGlobalRole()).Returns(GlobalRole.User);
        mockCurrentUserService.Setup(s => s.IsAdmin()).Returns(false);
        mockCurrentUserService.Setup(s => s.IsSuperAdmin()).Returns(false);

        // Replace ICurrentUserService in the scope
        var serviceProvider = scope.ServiceProvider;
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Create a custom service provider that uses the mock
        var services = new ServiceCollection();
        foreach (var service in serviceProvider.GetServices<ICurrentUserService>())
        {
            services.AddSingleton(service);
        }
        services.AddSingleton(mockCurrentUserService.Object);
        services.AddSingleton(serviceProvider.GetRequiredService<IMediator>());
        services.AddSingleton(serviceProvider.GetRequiredService<IUnitOfWork>());
        services.AddSingleton(serviceProvider.GetRequiredService<ILogger<SearchQueryHandler>>());

        // Use reflection or create handler directly with mocked service
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        var logger = serviceProvider.GetRequiredService<ILogger<SearchQueryHandler>>();
        var handler = new SearchQueryHandler(unitOfWork, mockCurrentUserService.Object, logger);

        // Act
        var query = new SearchQuery("Project", 20);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert - Should only return Org1 project
        result.Results.Should().HaveCount(1);
        result.Results.Should().OnlyContain(r => r.Type == "project" && r.Id == _project1Org1.Id);
        result.Results.Should().NotContain(r => r.Id == _project2Org2.Id);
    }

    [Fact]
    public async TaskAsync SearchTasks_UserFromOrg1_OnlyReturnsOrg1Tasks()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_user1Org1.Id);
        mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(_org1.Id);
        mockCurrentUserService.Setup(s => s.GetGlobalRole()).Returns(GlobalRole.User);
        mockCurrentUserService.Setup(s => s.IsAdmin()).Returns(false);
        mockCurrentUserService.Setup(s => s.IsSuperAdmin()).Returns(false);

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchQueryHandler>>();
        var handler = new SearchQueryHandler(unitOfWork, mockCurrentUserService.Object, logger);

        // Act
        var query = new SearchQuery("Task", 20);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert - Should only return Org1 task
        result.Results.Should().HaveCount(1);
        result.Results.Should().OnlyContain(r => r.Type == "task" && r.Id == _task1Org1.Id);
        result.Results.Should().NotContain(r => r.Id == _task2Org2.Id);
    }

    [Fact]
    public async TaskAsync SearchUsers_UserFromOrg1_OnlyReturnsOrg1Users()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_user1Org1.Id);
        mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(_org1.Id);
        mockCurrentUserService.Setup(s => s.GetGlobalRole()).Returns(GlobalRole.User);
        mockCurrentUserService.Setup(s => s.IsAdmin()).Returns(false);
        mockCurrentUserService.Setup(s => s.IsSuperAdmin()).Returns(false);

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchQueryHandler>>();
        var handler = new SearchQueryHandler(unitOfWork, mockCurrentUserService.Object, logger);

        // Act
        var query = new SearchQuery("User", 20);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert - Should only return Org1 users (user1org1 and user2org1)
        result.Results.Should().HaveCount(2);
        result.Results.Should().OnlyContain(r => r.Type == "user" && 
            (r.Id == _user1Org1.Id || r.Id == _user2Org1.Id));
        result.Results.Should().NotContain(r => r.Id == _user1Org2.Id);
    }

    [Fact]
    public async TaskAsync SearchComments_UserFromOrg1_OnlyReturnsOrg1Comments()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_user1Org1.Id);
        mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(_org1.Id);
        mockCurrentUserService.Setup(s => s.GetGlobalRole()).Returns(GlobalRole.User);
        mockCurrentUserService.Setup(s => s.IsAdmin()).Returns(false);
        mockCurrentUserService.Setup(s => s.IsSuperAdmin()).Returns(false);

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchQueryHandler>>();
        var handler = new SearchQueryHandler(unitOfWork, mockCurrentUserService.Object, logger);

        // Act
        var query = new SearchQuery("comment", 20);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert - Should only return Org1 comment
        result.Results.Should().HaveCount(1);
        result.Results.Should().OnlyContain(r => r.Type == "comment" && r.Id == _comment1Org1.Id);
        result.Results.Should().NotContain(r => r.Id == _comment2Org2.Id);
    }

    [Fact]
    public async TaskAsync Search_UserFromOrg2_OnlyReturnsOrg2Data()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_user1Org2.Id);
        mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(_org2.Id);
        mockCurrentUserService.Setup(s => s.GetGlobalRole()).Returns(GlobalRole.User);
        mockCurrentUserService.Setup(s => s.IsAdmin()).Returns(false);
        mockCurrentUserService.Setup(s => s.IsSuperAdmin()).Returns(false);

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchQueryHandler>>();
        var handler = new SearchQueryHandler(unitOfWork, mockCurrentUserService.Object, logger);

        // Act - Search for "Beta" which exists in Org2
        var query = new SearchQuery("Beta", 20);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert - Should only return Org2 data
        result.Results.Should().NotBeEmpty();
        result.Results.Should().OnlyContain(r => 
            (r.Type == "project" && r.Id == _project2Org2.Id) ||
            (r.Type == "task" && r.Id == _task2Org2.Id) ||
            (r.Type == "comment" && r.Id == _comment2Org2.Id) ||
            (r.Type == "user" && r.Id == _user1Org2.Id));
        
        // Should NOT contain any Org1 data
        result.Results.Should().NotContain(r => r.Id == _project1Org1.Id);
        result.Results.Should().NotContain(r => r.Id == _task1Org1.Id);
        result.Results.Should().NotContain(r => r.Id == _comment1Org1.Id);
        result.Results.Should().NotContain(r => r.Id == _user1Org1.Id || r.Id == _user2Org1.Id);
    }

    [Fact]
    public async TaskAsync Search_UserWithoutOrganization_ReturnsEmptyResults()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetUserId()).Returns(999);
        mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(0); // No organization
        mockCurrentUserService.Setup(s => s.GetGlobalRole()).Returns(GlobalRole.User);
        mockCurrentUserService.Setup(s => s.IsAdmin()).Returns(false);
        mockCurrentUserService.Setup(s => s.IsSuperAdmin()).Returns(false);

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchQueryHandler>>();
        var handler = new SearchQueryHandler(unitOfWork, mockCurrentUserService.Object, logger);

        // Act
        var query = new SearchQuery("Project", 20);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert - Should return empty results when user has no organization
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async TaskAsync Search_EmptyQuery_ReturnsEmptyResults()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_user1Org1.Id);
        mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(_org1.Id);

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchQueryHandler>>();
        var handler = new SearchQueryHandler(unitOfWork, mockCurrentUserService.Object, logger);

        // Act
        var query = new SearchQuery("", 20);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async TaskAsync Search_SpecialCharacters_DoesNotExposeCrossTenantData()
    {
        // Arrange - Test that special characters don't bypass organization filter
        using var scope = _factory.Services.CreateScope();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_user1Org1.Id);
        mockCurrentUserService.Setup(s => s.GetOrganizationId()).Returns(_org1.Id);
        mockCurrentUserService.Setup(s => s.GetGlobalRole()).Returns(GlobalRole.User);
        mockCurrentUserService.Setup(s => s.IsAdmin()).Returns(false);
        mockCurrentUserService.Setup(s => s.IsSuperAdmin()).Returns(false);

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchQueryHandler>>();
        var handler = new SearchQueryHandler(unitOfWork, mockCurrentUserService.Object, logger);

        // Act - Try various special characters that might be used in SQL injection attempts
        var specialQueries = new[] { "'; DROP TABLE--", "%", "_", "\\", "' OR '1'='1" };
        
        foreach (var specialQuery in specialQueries)
        {
            var query = new SearchQuery(specialQuery, 20);
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert - Should never return Org2 data, even with special characters
            result.Results.Should().NotContain(r => 
                r.Id == _project2Org2.Id || 
                r.Id == _task2Org2.Id || 
                r.Id == _comment2Org2.Id || 
                r.Id == _user1Org2.Id);
        }
    }

    public void Dispose()
    {
        _seedScope?.Dispose();
    }
}
