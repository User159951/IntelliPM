using FluentAssertions;
using IntelliPM.Application.Comments.Commands;
using IntelliPM.Application.Comments.Queries;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Tests.Infrastructure.TestAuthentication;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for comment security:
/// - XSS injection prevention
/// - Cross-tenant access prevention
/// - Mention validation
/// </summary>
public class CommentSecurityTests : IClassFixture<AIAgentHandlerTestFactory>, IDisposable
{
    private readonly AIAgentHandlerTestFactory _factory;
    private readonly IServiceScope _seedScope;
    private readonly AppDbContext _seedDbContext;
    private readonly IMediator _mediator;

    private Organization _org1 = null!;
    private Organization _org2 = null!;
    private User _user1 = null!;
    private User _user2 = null!;
    private Project _project1 = null!;
    private Project _project2 = null!;
    private ProjectTask _task1 = null!;
    private ProjectTask _task2 = null!;

    public CommentSecurityTests(AIAgentHandlerTestFactory factory)
    {
        _factory = factory;
        _seedScope = factory.Services.CreateScope();
        _seedDbContext = _seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
        _mediator = _seedScope.ServiceProvider.GetRequiredService<IMediator>();

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
            OrganizationId = _org1.Id,
            IsActive = true,
            GlobalRole = GlobalRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _user2 = new User
        {
            Email = "user2@org2.com",
            Username = "user2",
            FirstName = "User",
            LastName = "Two",
            OrganizationId = _org2.Id,
            IsActive = true,
            GlobalRole = GlobalRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _seedDbContext.Users.AddRange(_user1, _user2);
        _seedDbContext.SaveChanges();

        // Create projects
        _project1 = new Project
        {
            Name = "Project 1",
            OrganizationId = _org1.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _project2 = new Project
        {
            Name = "Project 2",
            OrganizationId = _org2.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _seedDbContext.Projects.AddRange(_project1, _project2);
        _seedDbContext.SaveChanges();

        // Create tasks
        _task1 = new ProjectTask
        {
            Title = "Task 1",
            OrganizationId = _org1.Id,
            ProjectId = _project1.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _task2 = new ProjectTask
        {
            Title = "Task 2",
            OrganizationId = _org2.Id,
            ProjectId = _project2.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _seedDbContext.ProjectTasks.AddRange(_task1, _task2);
        _seedDbContext.SaveChanges();
    }

    #region XSS Injection Tests

    [Fact]
    public async Task AddComment_XssScriptTag_IsSanitized()
    {
        // Arrange
        var maliciousContent = "Hello <script>alert('xss')</script> world";
        var command = new AddCommentCommand
        {
            EntityType = "Task",
            EntityId = _task1.Id,
            Content = maliciousContent
        };

        // Set user context to org1
        SetUserContext(_user1.Id, _org1.Id, _user1.Username);
        var scope = _factory.Services.CreateScope();
        var currentUserService = scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.ICurrentUserService>();

        var handler = new AddCommentCommandHandler(
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.IUnitOfWork>(),
            currentUserService,
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.IMentionParser>(),
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.ICommentSanitizationService>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AddCommentCommandHandler>>()
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Content.ToLowerInvariant().Should().NotContain("<script>", because: "XSS script tags should be sanitized");
        result.Content.ToLowerInvariant().Should().NotContain("alert", because: "XSS alert calls should be sanitized");
    }

    [Fact]
    public async Task AddComment_JavaScriptProtocol_IsSanitized()
    {
        // Arrange
        var maliciousContent = "Check javascript:alert('xss')";
        var command = new AddCommentCommand
        {
            EntityType = "Task",
            EntityId = _task1.Id,
            Content = maliciousContent
        };

        SetUserContext(_user1.Id, _org1.Id, _user1.Username);
        var scope = _factory.Services.CreateScope();
        var currentUserService = scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.ICurrentUserService>();

        var handler = new AddCommentCommandHandler(
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.IUnitOfWork>(),
            currentUserService,
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.IMentionParser>(),
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.ICommentSanitizationService>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AddCommentCommandHandler>>()
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Content.ToLowerInvariant().Should().NotContain("javascript:", because: "JavaScript protocol should be sanitized");
    }

    [Fact]
    public async Task AddComment_OnClickHandler_IsSanitized()
    {
        // Arrange
        var maliciousContent = "Click <div onclick='alert(1)'>here</div>";
        var command = new AddCommentCommand
        {
            EntityType = "Task",
            EntityId = _task1.Id,
            Content = maliciousContent
        };

        SetUserContext(_user1.Id, _org1.Id, _user1.Username);
        var scope = _factory.Services.CreateScope();
        var currentUserService = scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.ICurrentUserService>();

        var handler = new AddCommentCommandHandler(
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.IUnitOfWork>(),
            currentUserService,
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.IMentionParser>(),
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.ICommentSanitizationService>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AddCommentCommandHandler>>()
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Content.ToLowerInvariant().Should().NotContain("onclick", because: "Event handlers should be sanitized");
        result.Content.ToLowerInvariant().Should().NotContain("<div", because: "HTML tags should be sanitized");
    }

    #endregion

    #region Cross-Tenant Access Tests

    [Fact]
    public async Task AddComment_EntityFromOtherOrg_ThrowsNotFoundException()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "Task",
            EntityId = _task2.Id, // Task from org2
            Content = "Test comment"
        };

        SetUserContext(_user1.Id, _org1.Id, _user1.Username);
        var scope = _factory.Services.CreateScope();
        var currentUserService = scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.ICurrentUserService>(); // User from org1

        var handler = new AddCommentCommandHandler(
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.IUnitOfWork>(),
            currentUserService,
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.IMentionParser>(),
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.ICommentSanitizationService>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AddCommentCommandHandler>>()
        );

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetComments_EntityFromOtherOrg_ReturnsEmptyList()
    {
        // Arrange
        // Create a comment on task1 (org1) first
        _seedDbContext.BypassTenantFilter = true;
        var comment = new Comment
        {
            OrganizationId = _org1.Id,
            EntityType = "Task",
            EntityId = _task1.Id,
            Content = "Test comment",
            AuthorId = _user1.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Comments.Add(comment);
        _seedDbContext.SaveChanges();
        _seedDbContext.BypassTenantFilter = false;

        var query = new GetCommentsQuery
        {
            EntityType = "Task",
            EntityId = _task2.Id, // Task from org2
            OrganizationId = _org1.Id // Querying from org1
        };

        var scope = _factory.Services.CreateScope();
        var handler = new GetCommentsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.IUnitOfWork>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<GetCommentsQueryHandler>>()
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty(); // Should return empty, not throw
    }

    [Fact]
    public async Task GetComments_CrossTenantComment_IsFiltered()
    {
        // Arrange
        // Create a comment on task1 but with org2's organization ID (simulating data corruption)
        _seedDbContext.BypassTenantFilter = true;
        var maliciousComment = new Comment
        {
            OrganizationId = _org2.Id, // Wrong org ID
            EntityType = "Task",
            EntityId = _task1.Id, // But on org1's task
            Content = "Malicious comment",
            AuthorId = _user2.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _seedDbContext.Comments.Add(maliciousComment);
        _seedDbContext.SaveChanges();
        _seedDbContext.BypassTenantFilter = false;

        var query = new GetCommentsQuery
        {
            EntityType = "Task",
            EntityId = _task1.Id,
            OrganizationId = _org1.Id // Querying from org1
        };

        var scope = _factory.Services.CreateScope();
        var handler = new GetCommentsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.IUnitOfWork>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<GetCommentsQueryHandler>>()
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty(); // Should be filtered out by OrganizationId check
    }

    #endregion

    #region Mention Validation Tests

    [Fact]
    public async Task AddComment_MentionUserFromOtherOrg_DoesNotNotify()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "Task",
            EntityId = _task1.Id,
            Content = $"Hey @{_user2.Username}, check this!" // Mentioning user from org2
        };

        SetUserContext(_user1.Id, _org1.Id, _user1.Username);
        var scope = _factory.Services.CreateScope();
        var currentUserService = scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.ICurrentUserService>(); // User from org1

        var handler = new AddCommentCommandHandler(
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Common.Interfaces.IUnitOfWork>(),
            currentUserService,
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.IMentionParser>(),
            scope.ServiceProvider.GetRequiredService<IntelliPM.Application.Services.ICommentSanitizationService>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AddCommentCommandHandler>>()
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        // User2 should not be in mentioned users because they're from a different org
        result.MentionedUserIds.Should().NotContain(_user2.Id);
    }

    #endregion

    private void SetUserContext(int userId, int orgId, string username, GlobalRole role = GlobalRole.User)
    {
        var claims = new List<Claim>
        {
            new Claim("userId", userId.ToString()),
            new Claim("organizationId", orgId.ToString()),
            new Claim("username", username),
            new Claim("role", role.ToString())
        };
        TestAuthenticationContext.SetClaims(claims);
    }

    public void Dispose()
    {
        TestAuthenticationContext.Clear();
        _seedScope?.Dispose();
    }
}
