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
        // Get or create permissions (avoid duplicate key errors if already seeded by Program.cs)
        var permissionNames = new[] { "projects.view", "projects.create", "projects.update", "projects.delete", 
            "tasks.view", "tasks.create", "tasks.update", "users.view", "admin.access", "sprints.view" };
        var permissions = new List<Permission>();

        foreach (var name in permissionNames)
        {
            var existing = await db.Permissions.FirstOrDefaultAsync(p => p.Name == name);
            if (existing == null)
            {
                var category = name.Split('.')[0] switch
                {
                    "projects" => "Projects",
                    "tasks" => "Tasks",
                    "users" => "Users",
                    "admin" => "Admin",
                    "sprints" => "Sprints",
                    _ => "General"
                };
                existing = new Permission { Name = name, Category = category, CreatedAt = DateTimeOffset.UtcNow };
                db.Permissions.Add(existing);
                await db.SaveChangesAsync();
            }
            permissions.Add(existing);
        }

        // Seed RolePermissions - Admin gets all permissions
        var adminRolePermissions = new List<RolePermission>();
        foreach (var permission in permissions)
        {
            var exists = await db.RolePermissions.AnyAsync(rp => 
                rp.Role == GlobalRole.Admin && rp.PermissionId == permission.Id);
            if (!exists)
            {
                adminRolePermissions.Add(new RolePermission
                {
                    Role = GlobalRole.Admin,
                    PermissionId = permission.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        // User role gets limited permissions
        var userPermissionNames = new[] { "projects.view", "projects.create", "tasks.view", "tasks.create", 
            "tasks.update", "users.view", "sprints.view" };
        var userRolePermissions = new List<RolePermission>();
        foreach (var permissionName in userPermissionNames)
        {
            var permission = permissions.First(p => p.Name == permissionName);
            var exists = await db.RolePermissions.AnyAsync(rp => 
                rp.Role == GlobalRole.User && rp.PermissionId == permission.Id);
            if (!exists)
            {
                userRolePermissions.Add(new RolePermission
                {
                    Role = GlobalRole.User,
                    PermissionId = permission.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        if (adminRolePermissions.Any())
        {
            db.RolePermissions.AddRange(adminRolePermissions);
        }
        if (userRolePermissions.Any())
        {
            db.RolePermissions.AddRange(userRolePermissions);
        }
        if (adminRolePermissions.Any() || userRolePermissions.Any())
        {
            await db.SaveChangesAsync();
        }
    }

    #endregion
}

