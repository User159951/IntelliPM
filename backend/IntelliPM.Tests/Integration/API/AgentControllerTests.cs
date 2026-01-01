using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Application.Agent.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.API;

public class AgentControllerTests : IClassFixture<AIAgentApiTestFactory>
{
    private readonly AIAgentApiTestFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AgentControllerTests(AIAgentApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region POST /api/v1/Agent/improve-task

    [Fact]
    public async SystemTask ImproveTask_WithValidDescription_Returns200WithImprovedTask()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test@example.com", "testuser", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { Description = "Make login work" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Agent/improve-task", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AgentResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
        result.Content.Should().NotBeNullOrEmpty();
        result.ExecutionTimeMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async SystemTask ImproveTask_WithEmptyDescription_Returns400()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test2@example.com", "testuser2", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { Description = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Agent/improve-task", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async SystemTask ImproveTask_WithoutAuth_Returns401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var request = new { Description = "Test task" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Agent/improve-task", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/v1/Agent/analyze-project/{projectId}

    [Fact]
    public async SystemTask AnalyzeProject_WithValidProject_Returns200WithAnalysis()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test3@example.com", "testuser3", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = CreateTestProject("Test Project", user.Id, org.Id);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/v1/Agent/analyze-project/{project.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AgentResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
        result.Content.Should().NotBeNullOrEmpty();
        result.ExecutionTimeMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async SystemTask AnalyzeProject_WithNonExistentProject_Returns500()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test4@example.com", "testuser4", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var nonExistentProjectId = 99999;

        // Act
        var response = await _client.PostAsync($"/api/v1/Agent/analyze-project/{nonExistentProjectId}", null);

        // Assert
        // The handler returns 500 when project not found
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region POST /api/v1/Agent/detect-risks/{projectId}

    [Fact]
    public async SystemTask DetectRisks_WithValidProject_Returns200WithRisks()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test5@example.com", "testuser5", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = CreateTestProject("Risk Test Project", user.Id, org.Id);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/v1/Agent/detect-risks/{project.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AgentResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
        result.Content.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region POST /api/v1/Agent/plan-sprint/{sprintId}

    [Fact]
    public async SystemTask PlanSprint_WithValidSprint_Returns200WithPlan()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test6@example.com", "testuser6", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = CreateTestProject("Sprint Test Project", user.Id, org.Id);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var sprint = new Sprint
        {
            ProjectId = project.Id,
            OrganizationId = org.Id,
            Number = 1,
            Goal = "Complete authentication",
            StartDate = DateTimeOffset.UtcNow,
            EndDate = DateTimeOffset.UtcNow.AddDays(14),
            Status = "Planned",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/v1/Agent/plan-sprint/{sprint.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AgentResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
        result.Content.Should().NotBeNullOrEmpty();
        result.RequiresApproval.Should().BeTrue(); // Sprint planning requires approval
    }

    #endregion

    #region GET /api/v1/Agent/audit-log

    [Fact]
    public async SystemTask GetAuditLog_WithValidRequest_Returns200WithLogs()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test7@example.com", "testuser7", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Create some audit logs
        var log1 = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            AgentId = "project-insight",
            UserId = user.Id.ToString(),
            UserInput = "Analyze project 1",
            AgentResponse = "Analysis complete",
            Status = "Success",
            ExecutionTimeMs = 100,
            ExecutionCostUsd = 0.01m,
            CreatedAt = DateTime.UtcNow
        };
        db.AgentExecutionLogs.Add(log1);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Agent/audit-log?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetAgentAuditLogsResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Logs.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async SystemTask GetAuditLog_WithInvalidPage_Returns400()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test8@example.com", "testuser8", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Agent/audit-log?page=0&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/v1/Agent/metrics

    [Fact]
    public async SystemTask GetMetrics_WithValidRequest_Returns200WithMetrics()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test9@example.com", "testuser9", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Agent/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AgentMetricsDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.TotalExecutions.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Helper Methods

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

    private string GenerateJwtToken(int userId, string username, string email)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, new List<string>());
    }

    #endregion
}

