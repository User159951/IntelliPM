using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IntelliPM.Application.AI.Queries;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Constants;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.API;

public class AIGovernanceControllerTests : IClassFixture<AIAgentApiTestFactory>
{
    private readonly AIAgentApiTestFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AIGovernanceControllerTests(AIAgentApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GET /api/v1/ai/decisions

    [Fact]
    public async SystemTask GetDecisions_WithValidRequest_Returns200WithPagedDecisions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test@example.com", "testuser", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Create AI decision log
        var decisionLog = new AIDecisionLog
        {
            OrganizationId = org.Id,
            DecisionId = Guid.NewGuid(),
            DecisionType = AIDecisionConstants.DecisionTypes.BacklogPrioritization,
            AgentType = AIDecisionConstants.AgentTypes.ProductAgent,
            EntityType = "Project",
            EntityId = 1,
            EntityName = "Test Project",
            Question = "Prioritize backlog items",
            Decision = "{\"prioritized\": []}",
            Reasoning = "Based on ROI analysis",
            ConfidenceScore = 0.85m,
            ModelName = "llama3.2:3b",
            ModelVersion = "1.0",
            Status = AIDecisionConstants.Statuses.Applied,
            WasApplied = true,
            AppliedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            ExecutionTimeMs = 100,
            IsSuccess = true
        };
        db.AIDecisionLogs.Add(decisionLog);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/ai/decisions?page=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<AIDecisionLogDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async SystemTask GetDecisions_WithFilters_ReturnsFilteredDecisions()
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

        // Act
        var response = await _client.GetAsync($"/api/v1/ai/decisions?decisionType={AIDecisionConstants.DecisionTypes.BacklogPrioritization}&agentType={AIDecisionConstants.AgentTypes.ProductAgent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<AIDecisionLogDto>>(JsonOptions);
        result.Should().NotBeNull();
    }

    #endregion

    #region GET /api/v1/ai/decisions/{decisionId}

    [Fact]
    public async SystemTask GetDecisionById_WithValidDecisionId_Returns200WithDecision()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test3@example.com", "testuser3", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var decisionId = Guid.NewGuid();
        var decisionLog = new AIDecisionLog
        {
            OrganizationId = org.Id,
            DecisionId = decisionId,
            DecisionType = AIDecisionConstants.DecisionTypes.BacklogPrioritization,
            AgentType = AIDecisionConstants.AgentTypes.ProductAgent,
            EntityType = "Project",
            EntityId = 1,
            EntityName = "Test Project",
            Question = "Prioritize backlog items",
            Decision = "{\"prioritized\": []}",
            Reasoning = "Based on ROI analysis",
            ConfidenceScore = 0.85m,
            ModelName = "llama3.2:3b",
            ModelVersion = "1.0",
            Status = AIDecisionConstants.Statuses.Applied,
            WasApplied = true,
            AppliedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            ExecutionTimeMs = 100,
            IsSuccess = true
        };
        db.AIDecisionLogs.Add(decisionLog);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/ai/decisions/{decisionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AIDecisionLogDetailDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.DecisionId.Should().Be(decisionId);
        result.DecisionType.Should().Be(AIDecisionConstants.DecisionTypes.BacklogPrioritization);
    }

    [Fact]
    public async SystemTask GetDecisionById_WithNonExistentDecisionId_Returns404()
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

        var nonExistentDecisionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/ai/decisions/{nonExistentDecisionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/v1/ai/decisions/{decisionId}/approve

    [Fact]
    public async SystemTask ApproveDecision_WithValidDecisionId_Returns200()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test5@example.com", "testuser5", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var decisionId = Guid.NewGuid();
        var decisionLog = new AIDecisionLog
        {
            OrganizationId = org.Id,
            DecisionId = decisionId,
            DecisionType = AIDecisionConstants.DecisionTypes.SprintPlanning,
            AgentType = AIDecisionConstants.AgentTypes.DeliveryAgent,
            EntityType = "Project",
            EntityId = 1,
            EntityName = "Test Project",
            Question = "Plan sprint",
            Decision = "{\"tasks\": []}",
            Reasoning = "Based on capacity",
            ConfidenceScore = 0.75m,
            ModelName = "llama3.2:3b",
            ModelVersion = "1.0",
            Status = AIDecisionConstants.Statuses.PendingApproval,
            RequiresHumanApproval = true,
            WasApplied = false,
            CreatedAt = DateTimeOffset.UtcNow,
            ExecutionTimeMs = 100,
            IsSuccess = true
        };
        db.AIDecisionLogs.Add(decisionLog);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { Notes = "Approved after review" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/ai/decisions/{decisionId}/approve", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /api/v1/ai/decisions/{decisionId}/reject

    [Fact]
    public async SystemTask RejectDecision_WithValidDecisionId_Returns200()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test6@example.com", "testuser6", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var decisionId = Guid.NewGuid();
        var decisionLog = new AIDecisionLog
        {
            OrganizationId = org.Id,
            DecisionId = decisionId,
            DecisionType = AIDecisionConstants.DecisionTypes.SprintPlanning,
            AgentType = AIDecisionConstants.AgentTypes.DeliveryAgent,
            EntityType = "Project",
            EntityId = 1,
            EntityName = "Test Project",
            Question = "Plan sprint",
            Decision = "{\"tasks\": []}",
            Reasoning = "Based on capacity",
            ConfidenceScore = 0.65m,
            ModelName = "llama3.2:3b",
            ModelVersion = "1.0",
            Status = AIDecisionConstants.Statuses.PendingApproval,
            RequiresHumanApproval = true,
            WasApplied = false,
            CreatedAt = DateTimeOffset.UtcNow,
            ExecutionTimeMs = 100,
            IsSuccess = true
        };
        db.AIDecisionLogs.Add(decisionLog);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new { Notes = "Rejected due to capacity concerns", Reason = "Insufficient team capacity" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/ai/decisions/{decisionId}/reject", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/v1/ai/quota

    [Fact]
    public async SystemTask GetQuotaStatus_WithValidRequest_Returns200WithQuotaStatus()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test7@example.com", "testuser7", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Create AI quota
        var quota = new AIQuota
        {
            OrganizationId = org.Id,
            TierName = "Basic",
            IsActive = true,
            PeriodStartDate = DateTimeOffset.UtcNow,
            PeriodEndDate = DateTimeOffset.UtcNow.AddDays(30),
            MaxTokensPerPeriod = 100000,
            MaxRequestsPerPeriod = 1000,
            MaxCostPerPeriod = 100.00m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.AIQuotas.Add(quota);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/ai/quota");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AIQuotaStatusDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.TierName.Should().NotBeNullOrEmpty();
        result.Usage.Should().NotBeNull();
    }

    #endregion

    #region GET /api/v1/ai/usage/statistics

    [Fact]
    public async SystemTask GetUsageStatistics_WithValidRequest_Returns200WithStatistics()
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

        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;

        // Act
        var response = await _client.GetAsync($"/api/v1/ai/usage/statistics?startDate={startDate:O}&endDate={endDate:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AIUsageStatisticsDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.TotalTokensUsed.Should().BeGreaterOrEqualTo(0);
        result.TotalRequests.Should().BeGreaterOrEqualTo(0);
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

    private string GenerateJwtToken(int userId, string username, string email)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, new List<string>());
    }

    #endregion
}

