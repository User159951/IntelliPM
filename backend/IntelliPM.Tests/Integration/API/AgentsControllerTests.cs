using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Constants;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.API;

public class AgentsControllerTests : IClassFixture<AIAgentApiTestFactory>
{
    private readonly AIAgentApiTestFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AgentsControllerTests(AIAgentApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region POST /api/v1/projects/{projectId}/agents/run-product

    [Fact]
    public async SystemTask RunProductAgent_WithValidProject_Returns200WithPrioritizedItems()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test@example.com", "testuser", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = CreateTestProject("Product Agent Test", user.Id, org.Id);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add backlog items
        var backlogItems = new List<BacklogItem>
        {
            new BacklogItem { ProjectId = project.Id, Title = "Feature A", Status = "Backlog", CreatedAt = DateTimeOffset.UtcNow },
            new BacklogItem { ProjectId = project.Id, Title = "Feature B", Status = "Backlog", CreatedAt = DateTimeOffset.UtcNow }
        };
        db.BacklogItems.AddRange(backlogItems);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/v1/projects/{project.Id}/agents/run-product", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProductAgentOutput>(JsonOptions);
        result.Should().NotBeNull();
        result!.PrioritizedItems.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0);
        result.Rationale.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async SystemTask RunProductAgent_WithNonExistentProject_Returns404()
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

        var nonExistentProjectId = 99999;

        // Act
        var response = await _client.PostAsync($"/api/v1/projects/{nonExistentProjectId}/agents/run-product", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/v1/projects/{projectId}/agents/run-delivery

    [Fact]
    public async SystemTask RunDeliveryAgent_WithValidProject_Returns200WithRiskAssessment()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test3@example.com", "testuser3", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = CreateTestProject("Delivery Agent Test", user.Id, org.Id);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/v1/projects/{project.Id}/agents/run-delivery", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DeliveryAgentOutput>(JsonOptions);
        result.Should().NotBeNull();
        result!.RiskAssessment.Should().NotBeNullOrEmpty();
        result.RecommendedActions.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0);
    }

    #endregion

    #region POST /api/v1/projects/{projectId}/agents/run-manager

    [Fact]
    public async SystemTask RunManagerAgent_WithValidProject_Returns200WithExecutiveSummary()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test4@example.com", "testuser4", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = CreateTestProject("Manager Agent Test", user.Id, org.Id);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/v1/projects/{project.Id}/agents/run-manager", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ManagerAgentOutput>(JsonOptions);
        result.Should().NotBeNull();
        result!.ExecutiveSummary.Should().NotBeNullOrEmpty();
        result.KeyDecisionsNeeded.Should().NotBeNull();
        result.Highlights.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0);
    }

    #endregion

    #region POST /api/v1/projects/{projectId}/agents/run-qa

    [Fact]
    public async SystemTask RunQAAgent_WithValidProject_Returns200WithDefectAnalysis()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test5@example.com", "testuser5", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = CreateTestProject("QA Agent Test", user.Id, org.Id);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Add some defects
        var defects = new List<Defect>
        {
            new Defect { ProjectId = project.Id, Title = "Bug 1", Status = "Open", Severity = "High", CreatedAt = DateTimeOffset.UtcNow },
            new Defect { ProjectId = project.Id, Title = "Bug 2", Status = "Open", Severity = "Medium", CreatedAt = DateTimeOffset.UtcNow }
        };
        db.Defects.AddRange(defects);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/v1/projects/{project.Id}/agents/run-qa", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QAAgentOutput>(JsonOptions);
        result.Should().NotBeNull();
        result!.DefectAnalysis.Should().NotBeNullOrEmpty();
        result.Patterns.Should().NotBeNull();
        result.Recommendations.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0);
    }

    #endregion

    #region POST /api/v1/projects/{projectId}/agents/run-business

    [Fact]
    public async SystemTask RunBusinessAgent_WithValidProject_Returns200WithValueSummary()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test6@example.com", "testuser6", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = CreateTestProject("Business Agent Test", user.Id, org.Id);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/v1/projects/{project.Id}/agents/run-business", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BusinessAgentOutput>(JsonOptions);
        result.Should().NotBeNull();
        result!.ValueDeliverySummary.Should().NotBeNullOrEmpty();
        result.ValueMetrics.Should().NotBeNull();
        result.BusinessHighlights.Should().NotBeNull();
        result.StrategicRecommendations.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0);
    }

    #endregion

    #region Helper Methods

    private async SystemTask<Organization> EnsureOrganizationExistsAsync(AppDbContext db)
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

