using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Application.Identity.Queries;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.API;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async SystemTask GetMe_WithValidToken_Returns200WithCurrentUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Create organization
        var organization = new Organization
        {
            Name = "Test Organization",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        // Create user
        var user = new User
        {
            Email = "test@test.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Seed permissions and role-permissions
        await SeedPermissionsAndRolePermissionsAsync(db);

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CurrentUserDto>();
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.GlobalRole.Should().Be(GlobalRole.User);
        result.OrganizationId.Should().Be(organization.Id);
        result.Permissions.Should().NotBeNull();
        // User role should have limited permissions (projects.view, projects.create, tasks.view, etc.)
        result.Permissions.Should().NotBeEmpty();
        result.Permissions.Should().Contain("projects.view");
        result.Permissions.Should().Contain("projects.create");
        result.Permissions.Should().Contain("tasks.view");
        // Admin permissions should not be present for regular users
        result.Permissions.Should().NotContain("admin.access");
    }

    [Fact]
    public async SystemTask GetMe_WithAdminRole_ReturnsAdminGlobalRole()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Create organization
        var organization = new Organization
        {
            Name = "Test Organization",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        // Create admin user
        var admin = new User
        {
            Email = "admin@test.com",
            Username = "admin",
            FirstName = "Admin",
            LastName = "User",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = organization.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        // Seed permissions and role-permissions
        await SeedPermissionsAndRolePermissionsAsync(db);

        var token = GenerateJwtToken(admin.Id, admin.Username, admin.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CurrentUserDto>();
        result.Should().NotBeNull();
        result!.GlobalRole.Should().Be(GlobalRole.Admin);
        result.OrganizationId.Should().Be(organization.Id);
        result.Permissions.Should().NotBeNull();
        // Admin role should have all permissions
        result.Permissions.Should().NotBeEmpty();
        result.Permissions.Should().Contain("admin.access");
        result.Permissions.Should().Contain("projects.create");
        result.Permissions.Should().Contain("users.view");
    }

    [Fact]
    public async SystemTask GetMe_WithoutAuthentication_Returns401()
    {
        // Arrange - No token set

        // Act
        var response = await _client.GetAsync("/api/v1/Auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async SystemTask GetMe_WithInvalidToken_Returns401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/v1/Auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #region Helper Methods

    private string GenerateJwtToken(int userId, string username, string email)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, new List<string>());
    }

    private async SystemTask SeedPermissionsAndRolePermissionsAsync(AppDbContext db)
    {
        // Seed Permissions
        var permissions = new[]
        {
            new Permission { Id = 1, Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 2, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 3, Name = "projects.update", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 4, Name = "projects.delete", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 5, Name = "tasks.view", Category = "Tasks", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 6, Name = "tasks.create", Category = "Tasks", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 7, Name = "tasks.update", Category = "Tasks", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 8, Name = "users.view", Category = "Users", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 9, Name = "admin.access", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 10, Name = "sprints.view", Category = "Sprints", CreatedAt = DateTimeOffset.UtcNow }
        };

        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();

        // Seed RolePermissions - Admin gets all permissions
        var adminRolePermissions = permissions.Select(p => new RolePermission
        {
            Role = GlobalRole.Admin,
            PermissionId = p.Id,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        // User role gets limited permissions
        var userRolePermissions = new[]
        {
            new RolePermission { Role = GlobalRole.User, PermissionId = 1, CreatedAt = DateTimeOffset.UtcNow }, // projects.view
            new RolePermission { Role = GlobalRole.User, PermissionId = 2, CreatedAt = DateTimeOffset.UtcNow }, // projects.create
            new RolePermission { Role = GlobalRole.User, PermissionId = 5, CreatedAt = DateTimeOffset.UtcNow }, // tasks.view
            new RolePermission { Role = GlobalRole.User, PermissionId = 6, CreatedAt = DateTimeOffset.UtcNow }, // tasks.create
            new RolePermission { Role = GlobalRole.User, PermissionId = 7, CreatedAt = DateTimeOffset.UtcNow }, // tasks.update
            new RolePermission { Role = GlobalRole.User, PermissionId = 8, CreatedAt = DateTimeOffset.UtcNow }, // users.view
            new RolePermission { Role = GlobalRole.User, PermissionId = 10, CreatedAt = DateTimeOffset.UtcNow } // sprints.view
        };

        db.RolePermissions.AddRange(adminRolePermissions);
        db.RolePermissions.AddRange(userRolePermissions);
        await db.SaveChangesAsync();
    }

    #endregion
}

