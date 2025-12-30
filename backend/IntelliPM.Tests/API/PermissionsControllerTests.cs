using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.API;

public class PermissionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PermissionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async SystemTask GetMatrix_AsAdmin_ReturnsPermissions()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var permissions = new[]
        {
            new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Name = "admin.permissions.update", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow },
        };
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();

        var admin = new User
        {
            Email = "admin@test.com",
            Username = "admin",
            FirstName = "Admin",
            LastName = "User",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = org.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt",
        };
        db.Users.Add(admin);
        db.RolePermissions.AddRange(
            new RolePermission { Role = GlobalRole.Admin, PermissionId = permissions[0].Id, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Role = GlobalRole.Admin, PermissionId = permissions[1].Id, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Role = GlobalRole.User, PermissionId = permissions[0].Id, CreatedAt = DateTimeOffset.UtcNow }
        );
        await db.SaveChangesAsync();

        db.Permissions.Count().Should().Be(2);
        db.RolePermissions.Count().Should().Be(3);

        var token = GenerateJwtToken(admin.Id, admin.Username, admin.Email, new[] { "Admin" });
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/permissions/matrix");
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GET matrix body: {body}");

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);
        var result = await response.Content.ReadFromJsonAsync<PermissionsMatrixResponse>();
        result.Should().NotBeNull();
        result!.Permissions.Should().HaveCount(2);
        result.RolePermissions.Should().ContainKey("Admin");
        result.RolePermissions["Admin"].Should().Contain(permissions[1].Id);
    }

    [Fact]
    public async SystemTask UpdateRolePermissions_AsAdmin_ReplacesMappings()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var perm1 = new Permission { Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        var perm2 = new Permission { Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        var permAdmin = new Permission { Name = "admin.permissions.update", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow };
        db.Permissions.AddRange(perm1, perm2, permAdmin);
        await db.SaveChangesAsync();

        var admin = new User
        {
            Email = "admin2@test.com",
            Username = "admin2",
            FirstName = "Admin",
            LastName = "User",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = org.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt",
        };
        db.Users.Add(admin);
        db.RolePermissions.Add(new RolePermission { Role = GlobalRole.User, PermissionId = perm1.Id, CreatedAt = DateTimeOffset.UtcNow });
        db.RolePermissions.Add(new RolePermission { Role = GlobalRole.Admin, PermissionId = perm2.Id, CreatedAt = DateTimeOffset.UtcNow });
        db.RolePermissions.Add(new RolePermission { Role = GlobalRole.Admin, PermissionId = permAdmin.Id, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(admin.Id, admin.Username, admin.Email, new[] { "Admin" });
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new { permissionIds = new[] { perm2.Id } };
        var response = await _client.PutAsJsonAsync($"/api/v1/permissions/roles/{GlobalRole.User}", payload);

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent, body);

        var updated = await db.RolePermissions
            .Where(rp => rp.Role == GlobalRole.User)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        updated.Should().BeEquivalentTo(new[] { perm2.Id });
    }

    [Fact]
    public async SystemTask GetMatrix_AsNonAdmin_ReturnsForbidden()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = new Organization { Name = "Org", CreatedAt = DateTimeOffset.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "user@test.com",
            Username = "user",
            FirstName = "Test",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = org.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt",
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/permissions/matrix");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private string GenerateJwtToken(int userId, string username, string email, IEnumerable<string>? roles = null)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, roles?.ToList() ?? new List<string>());
    }

    private record PermissionsMatrixResponse(
        List<PermissionItem> Permissions,
        Dictionary<string, List<int>> RolePermissions
    );

    private record PermissionItem(int Id, string Name, string Category, string? Description);
}

