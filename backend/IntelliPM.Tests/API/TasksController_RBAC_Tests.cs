using System;
using System.Net;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.API;

/// <summary>
/// RBAC integration tests for TasksController.
/// Verifies that endpoints return 403 Forbidden when users don't have the required permissions.
/// </summary>
public class TasksController_RBAC_Tests : IClassFixture<RBAC_WebApplicationFactory>
{
    private readonly RBAC_WebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TasksController_RBAC_Tests(RBAC_WebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GetBlockedTasks Tests

    [Fact]
    public async SystemTask TasksController_GetBlockedTasks_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // IMPORTANT: Clear existing RolePermissions for GlobalRole.User to ensure test isolation
        // This prevents permissions from other tests (like tasks.view) from affecting this test
        var existingUserRolePermissions = db.RolePermissions.Where(rp => rp.Role == GlobalRole.User);
        db.RolePermissions.RemoveRange(existingUserRolePermissions);
        await db.SaveChangesAsync();

        var org = new Organization { Name = "Test Org BlockedWithout", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        // Create user WITHOUT tasks.view permission
        var user = CreateTestUser($"blocked_without_{Guid.NewGuid()}@test.com", $"user_blocked_without_{Guid.NewGuid()}", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project BlockedWithout", user.Id, org.Id);
        
        db.Users.Add(user);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add some permissions but NOT tasks.view
        var projectsViewPerm = await db.Permissions.FirstOrDefaultAsync(p => p.Name == "projects.view")
            ?? new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        if (projectsViewPerm.Id == 0)
        {
            db.Permissions.Add(projectsViewPerm);
            await db.SaveChangesAsync();
        }

        // Give user only projects.view permission (not tasks.view)
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
        var response = await _client.GetAsync($"/api/v1/Tasks/project/{project.Id}/blocked");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async SystemTask TasksController_GetBlockedTasks_WithPermission_Returns200()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Test Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        // Create user WITH tasks.view permission
        var user = CreateTestUser("user@test.com", "user", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", user.Id, org.Id);
        
        db.Users.Add(user);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add tasks.view permission
        var tasksViewPerm = new Permission { Name = "tasks.view", Category = "Tasks", CreatedAt = DateTimeOffset.UtcNow };
        db.Permissions.Add(tasksViewPerm);
        await db.SaveChangesAsync();

        // Give user tasks.view permission
        db.RolePermissions.Add(new RolePermission
        {
            Role = GlobalRole.User,
            PermissionId = tasksViewPerm.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email, org.Id);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/Tasks/project/{project.Id}/blocked");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GetTasksByAssignee Tests

    [Fact]
    public async SystemTask TasksController_GetTasksByAssignee_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // IMPORTANT: Clear existing RolePermissions for GlobalRole.User to ensure test isolation
        // This prevents permissions from other tests (like tasks.view) from affecting this test
        var existingUserRolePermissions = db.RolePermissions.Where(rp => rp.Role == GlobalRole.User);
        db.RolePermissions.RemoveRange(existingUserRolePermissions);
        await db.SaveChangesAsync();

        var org = new Organization { Name = "Test Org AssigneeWithout", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        // Create user WITHOUT tasks.view permission
        var user = CreateTestUser($"assignee_without_{Guid.NewGuid()}@test.com", $"user_assignee_without_{Guid.NewGuid()}", GlobalRole.User, org.Id);
        var assignee = CreateTestUser($"assignee_{Guid.NewGuid()}@test.com", $"assignee_{Guid.NewGuid()}", GlobalRole.User, org.Id);
        
        db.Users.AddRange(user, assignee);
        await db.SaveChangesAsync();

        // Add some permissions but NOT tasks.view
        var projectsViewPerm = await db.Permissions.FirstOrDefaultAsync(p => p.Name == "projects.view")
            ?? new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        if (projectsViewPerm.Id == 0)
        {
            db.Permissions.Add(projectsViewPerm);
            await db.SaveChangesAsync();
        }

        // Give user only projects.view permission (not tasks.view)
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
        var response = await _client.GetAsync($"/api/v1/Tasks/assignee/{assignee.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async SystemTask TasksController_GetTasksByAssignee_WithPermission_Returns200()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Test Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        // Create user WITH tasks.view permission
        var user = CreateTestUser("user@test.com", "user", GlobalRole.User, org.Id);
        var assignee = CreateTestUser("assignee@test.com", "assignee", GlobalRole.User, org.Id);
        
        db.Users.AddRange(user, assignee);
        await db.SaveChangesAsync();

        // Add tasks.view permission
        var tasksViewPerm = new Permission { Name = "tasks.view", Category = "Tasks", CreatedAt = DateTimeOffset.UtcNow };
        db.Permissions.Add(tasksViewPerm);
        await db.SaveChangesAsync();

        // Give user tasks.view permission
        db.RolePermissions.Add(new RolePermission
        {
            Role = GlobalRole.User,
            PermissionId = tasksViewPerm.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email, org.Id);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/Tasks/assignee/{assignee.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
