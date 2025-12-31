using System;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Infrastructure.VectorStore;
using IntelliPM.Application.Projects.Queries;
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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = member.Id,
            Role = ProjectRole.Developer,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.AddRange(owner, member);
        db.Projects.Add(project);
        db.ProjectMembers.Add(projectMember);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(member.Id, "member", "member@test.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/Projects/{project.Id}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await response.Content.ReadFromJsonAsync<List<ProjectMemberDto>>();
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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var nonMember = CreateTestUser("nonmember@test.com", "nonmember", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        
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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var invitee = CreateTestUser("invitee@test.com", "invitee", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.AddRange(owner, invitee);
        db.Projects.Add(project);
        db.ProjectMembers.Add(projectMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.Add(owner);
        db.Projects.Add(project);
        db.ProjectMembers.Add(projectMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var developer = CreateTestUser("developer@test.com", "developer", GlobalRole.User);
        var invitee = CreateTestUser("invitee@test.com", "invitee", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = developer.Id,
            Role = ProjectRole.Developer,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.AddRange(owner, developer, invitee);
        db.Projects.Add(project);
        db.ProjectMembers.Add(projectMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var existingMember = CreateTestUser("existing@test.com", "existing", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        var existingProjectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = existingMember.Id,
            Role = ProjectRole.Developer,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.AddRange(owner, existingMember);
        db.Projects.Add(project);
        db.ProjectMembers.AddRange(ownerMember, existingProjectMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = member.Id,
            Role = ProjectRole.Developer,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.AddRange(owner, member);
        db.Projects.Add(project);
        db.ProjectMembers.AddRange(ownerMember, projectMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var scrumMaster = CreateTestUser("scrum@test.com", "scrum", GlobalRole.User);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        var scrumMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = scrumMaster.Id,
            Role = ProjectRole.ScrumMaster,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = member.Id,
            Role = ProjectRole.Developer,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.AddRange(owner, scrumMaster, member);
        db.Projects.Add(project);
        db.ProjectMembers.AddRange(ownerMember, scrumMember, projectMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.Add(owner);
        db.Projects.Add(project);
        db.ProjectMembers.Add(ownerMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = member.Id,
            Role = ProjectRole.Developer,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.AddRange(owner, member);
        db.Projects.Add(project);
        db.ProjectMembers.AddRange(ownerMember, projectMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var developer = CreateTestUser("developer@test.com", "developer", GlobalRole.User);
        var member = CreateTestUser("member@test.com", "member", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        var devMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = developer.Id,
            Role = ProjectRole.Developer,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = member.Id,
            Role = ProjectRole.Developer,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.AddRange(owner, developer, member);
        db.Projects.Add(project);
        db.ProjectMembers.AddRange(ownerMember, devMember, projectMember);
        await db.SaveChangesAsync();

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
        
        var owner = CreateTestUser("owner@test.com", "owner", GlobalRole.User);
        var project = CreateTestProject("Test Project", owner.Id);
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = owner.Id,
            Role = ProjectRole.ProductOwner,
            InvitedById = owner.Id,
            InvitedAt = DateTime.UtcNow
        };
        
        db.Users.Add(owner);
        db.Projects.Add(project);
        db.ProjectMembers.Add(ownerMember);
        await db.SaveChangesAsync();

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

