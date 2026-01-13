using System;
using System.Net;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Tests.Infrastructure.TestAuthentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.API;

/// <summary>
/// RBAC tests for ActivityController, SearchController, MetricsController, and InsightsController.
/// Verifies that endpoints return 403 Forbidden when users don't have the required permissions.
/// </summary>
public class RBAC_MissingControllers_Tests : IClassFixture<RBAC_WebApplicationFactory>
{
    private readonly RBAC_WebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RBAC_MissingControllers_Tests(RBAC_WebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region ActivityController Tests

    [Fact]
    public async SystemTask ActivityController_GetRecentActivity_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Test Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        // Create user WITHOUT activity.view permission
        var user = CreateTestUser("user@test.com", "user", GlobalRole.User, org.Id);
        db.Users.Add(user);

        // Add some permissions but NOT activity.view
        var projectsViewPerm = new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        db.Permissions.Add(projectsViewPerm);
        await db.SaveChangesAsync();

        // Give user only projects.view permission (not activity.view)
        db.RolePermissions.Add(new RolePermission
        {
            Role = GlobalRole.User,
            PermissionId = projectsViewPerm.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email, org.Id);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Activity/recent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region SearchController Tests

    [Fact]
    public async SystemTask SearchController_GlobalSearch_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Test Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        // Create user WITHOUT search.use permission
        var user = CreateTestUser("user@test.com", "user", GlobalRole.User, org.Id);
        db.Users.Add(user);

        // Add some permissions but NOT search.use
        var projectsViewPerm = new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        db.Permissions.Add(projectsViewPerm);
        await db.SaveChangesAsync();

        // Give user only projects.view permission (not search.use)
        db.RolePermissions.Add(new RolePermission
        {
            Role = GlobalRole.User,
            PermissionId = projectsViewPerm.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email, org.Id);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Search?q=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region MetricsController Tests

    [Fact]
    public async SystemTask MetricsController_GetMetrics_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Test Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        // Create user WITHOUT metrics.view permission
        var user = CreateTestUser("user@test.com", "user", GlobalRole.User, org.Id);
        db.Users.Add(user);

        // Add some permissions but NOT metrics.view
        var projectsViewPerm = new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        db.Permissions.Add(projectsViewPerm);
        await db.SaveChangesAsync();

        // Give user only projects.view permission (not metrics.view)
        db.RolePermissions.Add(new RolePermission
        {
            Role = GlobalRole.User,
            PermissionId = projectsViewPerm.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email, org.Id);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async SystemTask MetricsController_GetSprintVelocity_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Test Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var user = CreateTestUser("user@test.com", "user", GlobalRole.User, org.Id);
        db.Users.Add(user);

        var projectsViewPerm = new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        db.Permissions.Add(projectsViewPerm);
        await db.SaveChangesAsync();

        db.RolePermissions.Add(new RolePermission
        {
            Role = GlobalRole.User,
            PermissionId = projectsViewPerm.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email, org.Id);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Metrics/velocity?projectId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region InsightsController Tests

    [Fact]
    public async SystemTask InsightsController_GetInsights_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Test Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var user = CreateTestUser("user@test.com", "user", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", user.Id, org.Id);
        
        db.Users.Add(user);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add some permissions but NOT insights.view
        var projectsViewPerm = new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        db.Permissions.Add(projectsViewPerm);
        await db.SaveChangesAsync();

        db.RolePermissions.Add(new RolePermission
        {
            Role = GlobalRole.User,
            PermissionId = projectsViewPerm.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email, org.Id);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/projects/{project.Id}/insights");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Helper Methods

    private User CreateTestUser(string email, string username, GlobalRole role, int organizationId)
    {
        return new User
        {
            Email = email,
            Username = username,
            FirstName = "Test",
            LastName = "User",
            GlobalRole = role,
            OrganizationId = organizationId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
    }

    private Project CreateTestProject(string name, int ownerId, int organizationId)
    {
        return new Project
        {
            Name = name,
            Description = "Test Description",
            OwnerId = ownerId,
            OrganizationId = organizationId,
            Status = "Active",
            Type = "Scrum",
            SprintDurationDays = 14,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private string GenerateJwtToken(int userId, string username, string email, int organizationId)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        // Generate token - organizationId is retrieved from database via ICurrentUserService
        return tokenService.GenerateAccessToken(userId, username, email, new List<string>());
    }

    #endregion
}

/// <summary>
/// WebApplicationFactory for RBAC tests.
/// Configures in-memory database and test services.
/// </summary>
public class RBAC_WebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configure content root to use a temporary directory to avoid /app permission issues
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        builder.UseContentRoot(tempDir);
        
        // Set environment variables BEFORE any configuration is loaded
        // This ensures JWT SecretKey is available when Program.cs executes
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "YourSecureSecretKeyOfAt32CharactersMinimumForTesting!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TestAudience");
        
        // Configure app configuration - this MUST run before Program.cs executes
        // The issue is that ConfigureAppConfiguration may run after Program.cs checks the config
        // So we use a simpler approach: just add in-memory config which should work
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            // Add in-memory configuration
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "YourSecureSecretKeyOfAt32CharactersMinimumForTesting!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("RBAC_TestDb");
            });

            // Remove VectorDbContext registration if present
            var vectorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<IntelliPM.Infrastructure.VectorStore.VectorDbContext>));
            if (vectorDescriptor != null)
            {
                services.Remove(vectorDescriptor);
            }
            
            // Add InMemory VectorDbContext for testing
            services.AddDbContext<IntelliPM.Infrastructure.VectorStore.VectorDbContext>(options =>
            {
                options.UseInMemoryDatabase("RBAC_TestVectorDb");
            });

            // JWT configuration is already set in ConfigureAppConfiguration above

            // Add test authentication handler
            services.AddTestAuthentication();

            // Create database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        // Set environment to Testing so appsettings.Testing.json is loaded
        builder.UseEnvironment("Testing");
    }
}

