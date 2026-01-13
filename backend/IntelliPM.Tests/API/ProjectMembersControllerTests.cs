using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Infrastructure.VectorStore;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Tests.Infrastructure.TestAuthentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using DomainTask = IntelliPM.Domain.Entities.Task;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.API;

public class ProjectMembersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProjectMembersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GET /api/v1/Projects/{id}/members

    [Fact]
    public async SystemTask GetProjectMembers_WithValidProjectMember_Returns200WithMembers()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var org = await EnsureOrganizationExistsAsync(db);
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, member);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add both users as project members - member needs to be member to view, owner needs to be member too
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);
        await _factory.AddProjectMemberAsync(project.Id, member.Id, ProjectRole.Developer, owner.Id);

        var token = GenerateJwtToken(member.Id, "member", "member@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/Projects/{project.Id}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await DeserializeResponseAsync<List<ProjectMemberDto>>(response);
        members.Should().NotBeNull();
        members!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async SystemTask GetProjectMembers_WithoutAuthentication_Returns401()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/Projects/{project.Id}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async SystemTask GetProjectMembers_NotProjectMember_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var org = await EnsureOrganizationExistsAsync(db);
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var nonMember = CreateTestUser("nonmember@test.com", "nonmember", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, nonMember);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(nonMember.Id, "nonmember", "nonmember@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/Projects/{project.Id}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/v1/Projects/{id}/members

    [Fact]
    public async SystemTask InviteMember_WithValidEmailAndPermission_Returns201()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var invitee = CreateTestUser("invitee@test.com", "invitee", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, invitee);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add owner as ProductOwner so they can invite members
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);

        var token = GenerateJwtToken(owner.Id, "owner", "owner@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { Email = invitee.Email, Role = "Developer" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/Projects/{project.Id}/members", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async SystemTask InviteMember_WithInvalidEmail_Returns400()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add owner as ProductOwner
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);

        var token = GenerateJwtToken(owner.Id, "owner", "owner@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { Email = "invalid-email", Role = "Developer" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/Projects/{project.Id}/members", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async SystemTask InviteMember_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var developer = CreateTestUser("developer@test.com", "developer", GlobalRole.User, org.Id);
        var invitee = CreateTestUser("invitee@test.com", "invitee", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, developer, invitee);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add developer as member (they need to be a member to attempt invite, but won't have permission)
        await _factory.AddProjectMemberAsync(project.Id, developer.Id, ProjectRole.Developer, owner.Id);

        var token = GenerateJwtToken(developer.Id, "developer", "developer@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { Email = invitee.Email, Role = "Developer" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/Projects/{project.Id}/members", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async SystemTask InviteMember_AlreadyMember_Returns400()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var existingMember = CreateTestUser("existing@test.com", "existing", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, existingMember);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add both users as project members
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);
        await _factory.AddProjectMemberAsync(project.Id, existingMember.Id, ProjectRole.Developer, owner.Id);

        var token = GenerateJwtToken(owner.Id, "owner", "owner@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { Email = existingMember.Email, Role = "Developer" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/Projects/{project.Id}/members", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/v1/Projects/{projectId}/members/{userId}/role

    [Fact]
    public async SystemTask ChangeMemberRole_WithPermission_Returns204()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, member);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add both users as project members
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);
        await _factory.AddProjectMemberAsync(project.Id, member.Id, ProjectRole.Developer, owner.Id);

        var token = GenerateJwtToken(owner.Id, "owner", "owner@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { NewRole = "ScrumMaster" };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/Projects/{project.Id}/members/{member.Id}/role", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async SystemTask ChangeMemberRole_NotProductOwner_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var scrumMaster = CreateTestUser("scrum@test.com", "scrum", GlobalRole.User, org.Id);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, scrumMaster, member);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add all users as project members
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);
        await _factory.AddProjectMemberAsync(project.Id, scrumMaster.Id, ProjectRole.ScrumMaster, owner.Id);
        await _factory.AddProjectMemberAsync(project.Id, member.Id, ProjectRole.Developer, owner.Id);

        var token = GenerateJwtToken(scrumMaster.Id, "scrum", "scrum@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { NewRole = "Tester" };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/Projects/{project.Id}/members/{member.Id}/role", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async SystemTask ChangeMemberRole_TryingToChangeProductOwner_Returns400()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add owner as ProductOwner
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);

        var token = GenerateJwtToken(owner.Id, "owner", "owner@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { NewRole = "Developer" };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/Projects/{project.Id}/members/{owner.Id}/role", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE /api/v1/Projects/{projectId}/members/{userId}

    [Fact]
    public async SystemTask RemoveMember_WithPermission_Returns204()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, member);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add both users as project members
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);
        await _factory.AddProjectMemberAsync(project.Id, member.Id, ProjectRole.Developer, owner.Id);

        var token = GenerateJwtToken(owner.Id, "owner", "owner@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync(
            $"/api/v1/Projects/{project.Id}/members/{member.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async SystemTask RemoveMember_WithoutPermission_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var developer = CreateTestUser("developer@test.com", "developer", GlobalRole.User, org.Id);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.AddRange(owner, developer, member);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add all users as project members
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);
        await _factory.AddProjectMemberAsync(project.Id, developer.Id, ProjectRole.Developer, owner.Id);
        await _factory.AddProjectMemberAsync(project.Id, member.Id, ProjectRole.Developer, owner.Id);

        var token = GenerateJwtToken(developer.Id, "developer", "developer@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync(
            $"/api/v1/Projects/{project.Id}/members/{member.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async SystemTask RemoveMember_TryingToRemoveProductOwner_Returns400()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await SeedProjectMemberPermissionsAsync(db);
        var org = await EnsureOrganizationExistsAsync(db);
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User, org.Id);
        var project = CreateTestProject("Test Project", owner.Id, org.Id);
        
        db.Users.Add(owner);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add owner as ProductOwner
        await _factory.AddProjectMemberAsync(project.Id, owner.Id, ProjectRole.ProductOwner, owner.Id);

        var token = GenerateJwtToken(owner.Id, "owner", "owner@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync(
            $"/api/v1/Projects/{project.Id}/members/{owner.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Helper Methods

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async System.Threading.Tasks.Task<Organization> EnsureOrganizationExistsAsync(AppDbContext db)
    {
        var org = await db.Organizations.FirstOrDefaultAsync();
        if (org == null)
        {
            org = new Organization { Name = "Test Organization", CreatedAt = DateTimeOffset.UtcNow };
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
        }
        return org;
    }

    private User CreateTestUser(string email, string username, GlobalRole role, int organizationId = 1)
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

    private Project CreateTestProject(string name, int ownerId, int organizationId = 1)
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

    private string GenerateJwtToken(int userId, string username, string email)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, new List<string>());
    }

    private async SystemTask SeedProjectMemberPermissionsAsync(AppDbContext db)
    {
        // Ensure organization exists first
        await EnsureOrganizationExistsAsync(db);
        
        // Get or create permissions needed for project members operations
        var permissionNames = new[] { "projects.view", "projects.members.invite", "projects.members.changeRole", "projects.members.remove" };
        var permissions = new List<Permission>();

        foreach (var name in permissionNames)
        {
            var existing = await db.Permissions.FirstOrDefaultAsync(p => p.Name == name);
            if (existing == null)
            {
                var category = name.Split('.')[0] switch
                {
                    "projects" => "Projects",
                    _ => "General"
                };
                existing = new Permission { Name = name, Category = category, CreatedAt = DateTimeOffset.UtcNow };
                db.Permissions.Add(existing);
                await db.SaveChangesAsync();
            }
            permissions.Add(existing);
        }

        // Ensure User role has these permissions
        foreach (var permission in permissions)
        {
            var exists = await db.RolePermissions.AnyAsync(rp => 
                rp.Role == GlobalRole.User && rp.PermissionId == permission.Id);
            if (!exists)
            {
                db.RolePermissions.Add(new RolePermission
                {
                    Role = GlobalRole.User,
                    PermissionId = permission.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }
        
        await db.SaveChangesAsync();
    }

    #endregion
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
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
                options.UseInMemoryDatabase("TestDb");
            });

            // Remove VectorDbContext registration if present
            var vectorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<VectorDbContext>));
            if (vectorDescriptor != null)
            {
                services.Remove(vectorDescriptor);
            }
            
            // Add InMemory VectorDbContext for testing
            services.AddDbContext<VectorDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestVectorDb");
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

    /// <summary>
    /// Helper method to add a ProjectMember to a project
    /// </summary>
    public async Task<ProjectMember> AddProjectMemberAsync(
        int projectId,
        int userId,
        ProjectRole role,
        int? invitedById = null)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Use provided invitedById or default to userId
        var inviterId = invitedById ?? userId;

        var projectMember = new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            InvitedById = inviterId,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTimeOffset.UtcNow
        };

        context.ProjectMembers.Add(projectMember);
        await context.SaveChangesAsync();

        return projectMember;
    }
}

