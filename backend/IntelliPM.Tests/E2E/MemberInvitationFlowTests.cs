using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Application.Projects.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IntelliPM.Tests.API;
using DomainTask = IntelliPM.Domain.Entities.Task;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.E2E;

public class MemberInvitationFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MemberInvitationFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async SystemTask CompleteMemberInvitationFlow_EndToEnd_Success()
    {
        // Step 1: Register ProductOwner
        var productOwnerUsername = $"productowner_{Guid.NewGuid():N}";
        var productOwnerEmail = $"productowner_{Guid.NewGuid():N}@test.com";
        var productOwnerPassword = "ProductOwner123!";
        
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/Auth/register", new
        {
            Username = productOwnerUsername,
            Email = productOwnerEmail,
            Password = productOwnerPassword,
            FirstName = "Product",
            LastName = "Owner"
        });
        
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        registerResult.Should().NotBeNull();
        var productOwnerId = registerResult!.UserId;

        // Get auth token (API uses cookies but we'll generate token directly for tests)
        var authToken = await GetAuthTokenAsync(productOwnerUsername, productOwnerPassword);

        // Step 2: Create a new project as ProductOwner
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

        var createProjectRequest = new
        {
            Name = "E2E Test Project",
            Description = "Project for E2E member invitation test",
            Type = "Scrum",
            SprintDurationDays = 14
        };

        var createProjectResponse = await _client.PostAsJsonAsync("/api/v1/Projects", createProjectRequest);
        createProjectResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        project.Should().NotBeNull();
        var projectId = project!.Id;

        // Step 3: Register the user to be invited (they need to exist in the system)
        var invitedUserUsername = $"inviteduser_{Guid.NewGuid():N}";
        var invitedUserEmail = $"inviteduser_{Guid.NewGuid():N}@test.com";
        var invitedUserPassword = "InvitedUser123!";

        var invitedUserRegisterResponse = await _client.PostAsJsonAsync("/api/v1/Auth/register", new
        {
            Username = invitedUserUsername,
            Email = invitedUserEmail,
            Password = invitedUserPassword,
            FirstName = "Invited",
            LastName = "User"
        });

        invitedUserRegisterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var invitedUserRegisterResult = await invitedUserRegisterResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        invitedUserRegisterResult.Should().NotBeNull();
        var invitedUserId = invitedUserRegisterResult!.UserId;

        // Re-authenticate as ProductOwner
        authToken = await GetAuthTokenAsync(productOwnerUsername, productOwnerPassword);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

        // Step 4 & 5: Invite member (Navigate to project members page and fill form - simulated via API)
        var inviteRequest = new
        {
            Email = invitedUserEmail,
            Role = "Developer"
        };

        var inviteResponse = await _client.PostAsJsonAsync($"/api/v1/Projects/{projectId}/members", inviteRequest);
        
        // Step 6: Verify invitation was successful
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 7: Verify invitation appears in members list
        var membersResponse = await _client.GetAsync($"/api/v1/Projects/{projectId}/members");
        membersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await membersResponse.Content.ReadFromJsonAsync<List<ProjectMemberDto>>();
        members.Should().NotBeNull();
        members!.Should().Contain(m => m.Email == invitedUserEmail && m.Role == ProjectRole.Developer);

        // Step 8: Verify notification created for invited user
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.UserId == invitedUserId && n.ProjectId == projectId);
        
        notification.Should().NotBeNull();
        notification!.Type.Should().Be("project_invite");
        notification.IsRead.Should().BeFalse();
        notification.Message.Should().Contain("invited you to join project");

        // Step 9: Verify database has correct ProjectMember record
        var projectMember = await db.ProjectMembers
            .Include(pm => pm.User)
            .Include(pm => pm.InvitedBy)
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == invitedUserId);

        projectMember.Should().NotBeNull();
        projectMember!.Role.Should().Be(ProjectRole.Developer);
        projectMember.InvitedById.Should().Be(productOwnerId);
        projectMember.User.Email.Should().Be(invitedUserEmail);
        projectMember.InvitedBy.Email.Should().Be(productOwnerEmail);

        // Step 10: Logout and login as invited user
        _client.DefaultRequestHeaders.Remove("Authorization");
        
        var invitedUserAuthToken = await GetAuthTokenAsync(invitedUserUsername, invitedUserPassword);
        invitedUserAuthToken.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invitedUserAuthToken);

        // Step 11: Verify invited user can access the project
        var projectAccessResponse = await _client.GetAsync($"/api/v1/Projects/{projectId}");
        projectAccessResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var accessedProject = await projectAccessResponse.Content.ReadFromJsonAsync<GetProjectByIdResponse>();
        accessedProject.Should().NotBeNull();
        accessedProject!.Id.Should().Be(projectId);
        
        // Create a new scope for database access since we're reusing the client
        using var newScope = _factory.Services.CreateScope();
        var newDb = newScope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Step 12: Verify invited user has Developer role permissions
        // Check that user can see members (all roles can)
        var membersListResponse = await _client.GetAsync($"/api/v1/Projects/{projectId}/members");
        membersListResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Check user role via query (if endpoint exists) or via database
        var userRoleInProject = await newDb.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == invitedUserId);
        
        userRoleInProject.Should().NotBeNull();
        userRoleInProject!.Role.Should().Be(ProjectRole.Developer);

        // Verify Developer permissions:
        // - Can create tasks (Developer != Viewer)
        // - Cannot invite members (only ProductOwner/ScrumMaster)
        // - Cannot delete project (only ProductOwner)
        
        // Try to create a task (should succeed for Developer)
        var createTaskRequest = new
        {
            ProjectId = projectId,
            Title = "Test Task by Developer",
            Description = "Testing Developer permissions",
            Priority = "Medium"
        };

        var createTaskResponse = await _client.PostAsJsonAsync("/api/v1/Tasks", createTaskRequest);
        createTaskResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to invite a member (should fail - Developer cannot invite)
        var anotherUserEmail = $"anotheruser_{Guid.NewGuid():N}@test.com";
        var inviteAsDeveloperRequest = new
        {
            Email = anotherUserEmail,
            Role = "Tester"
        };

        var inviteAsDeveloperResponse = await _client.PostAsJsonAsync($"/api/v1/Projects/{projectId}/members", inviteAsDeveloperRequest);
        inviteAsDeveloperResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<string> GetAuthTokenAsync(string username, string password)
    {
        // Get user from database to generate token
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        user.Should().NotBeNull();
        
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(user!.Id, user.Username, user.Email, new List<string>());
    }

    private class RegisterResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // Use the actual response type from Application layer
    private class ProjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }

    private class ProjectMemberDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ProjectRole Role { get; set; }
        public DateTime InvitedAt { get; set; }
        public string InvitedByName { get; set; } = string.Empty;
    }
}

