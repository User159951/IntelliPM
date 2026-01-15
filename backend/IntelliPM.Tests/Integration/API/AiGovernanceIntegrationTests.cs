using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Tests.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.API;

/// <summary>
/// Integration tests for AI governance system:
/// - Quota exceeded scenarios
/// - Organization AI disabled scenarios
/// - Global AI kill switch scenarios
/// </summary>
public class AiGovernanceIntegrationTests : IClassFixture<AIAgentApiTestFactory>
{
    private readonly AIAgentApiTestFactory _factory;
    private readonly HttpClient _client;

    public AiGovernanceIntegrationTests(AIAgentApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async SystemTask AiExecution_WhenQuotaExceeded_Returns429()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Quota Test Org",
            Code = "quota-test-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "quota@test.com",
            Username = "quotauser",
            FirstName = "Quota",
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

        // Create quota that is exceeded
        var quotaTemplate = await db.AIQuotaTemplates.FirstOrDefaultAsync(t => t.TierName == "Free");
        if (quotaTemplate == null)
        {
            quotaTemplate = new AIQuotaTemplate
            {
                TierName = "Free",
                MaxTokensPerPeriod = 1000,
                MaxRequestsPerPeriod = 10,
                MaxDecisionsPerPeriod = 10,
                IsActive = true
            };
            db.AIQuotaTemplates.Add(quotaTemplate);
            await db.SaveChangesAsync();
        }

        var quota = new AIQuota
        {
            OrganizationId = organization.Id,
            TemplateId = quotaTemplate.Id,
            TierName = "Free",
            IsActive = true,
            EnforceQuota = true,
            MaxRequestsPerPeriod = 10,
            RequestsUsed = 10, // Exceeded
            PeriodStartDate = DateTimeOffset.UtcNow,
            PeriodEndDate = DateTimeOffset.UtcNow.AddDays(30),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.AIQuotas.Add(quota);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: Try to use AI feature
        var request = new { description = "Test task description" };
        var response = await _client.PostAsJsonAsync("/api/v1/Agent/improve-task", request);

        // Assert: Should return 429 Too Many Requests
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        var errorResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Detail.Should().Contain("limit exceeded");
    }

    [Fact]
    public async SystemTask AiExecution_WhenOrgAiDisabled_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Disabled AI Org",
            Code = "disabled-ai-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "disabled@test.com",
            Username = "disableduser",
            FirstName = "Disabled",
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

        // Create disabled quota
        var quotaTemplate = await db.AIQuotaTemplates.FirstOrDefaultAsync(t => t.TierName == "Disabled");
        if (quotaTemplate == null)
        {
            quotaTemplate = new AIQuotaTemplate
            {
                TierName = "Disabled",
                MaxTokensPerPeriod = 0,
                MaxRequestsPerPeriod = 0,
                MaxDecisionsPerPeriod = 0,
                IsActive = true
            };
            db.AIQuotaTemplates.Add(quotaTemplate);
            await db.SaveChangesAsync();
        }

        var disabledQuota = new AIQuota
        {
            OrganizationId = organization.Id,
            TemplateId = quotaTemplate.Id,
            TierName = "Disabled",
            IsActive = true,
            EnforceQuota = true,
            IsQuotaExceeded = true,
            QuotaExceededReason = "AI disabled for testing",
            PeriodStartDate = DateTimeOffset.UtcNow,
            PeriodEndDate = DateTimeOffset.MaxValue,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.AIQuotas.Add(disabledQuota);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: Try to use AI feature
        var request = new { description = "Test task description" };
        var response = await _client.PostAsJsonAsync("/api/v1/Agent/improve-task", request);

        // Assert: Should return 403 Forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var errorResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Detail.Should().Contain("disabled");
    }

    [Fact]
    public async SystemTask AiExecution_WhenGlobalAiDisabled_Returns403()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Global Disabled Org",
            Code = "global-disabled-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "globaldisabled@test.com",
            Username = "globaldisableduser",
            FirstName = "Global",
            LastName = "Disabled",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Create normal quota (not disabled)
        var quotaTemplate = await db.AIQuotaTemplates.FirstOrDefaultAsync(t => t.TierName == "Free");
        if (quotaTemplate == null)
        {
            quotaTemplate = new AIQuotaTemplate
            {
                TierName = "Free",
                MaxTokensPerPeriod = 1000,
                MaxRequestsPerPeriod = 100,
                MaxDecisionsPerPeriod = 100,
                IsActive = true
            };
            db.AIQuotaTemplates.Add(quotaTemplate);
            await db.SaveChangesAsync();
        }

        var quota = new AIQuota
        {
            OrganizationId = organization.Id,
            TemplateId = quotaTemplate.Id,
            TierName = "Free",
            IsActive = true,
            EnforceQuota = true,
            MaxRequestsPerPeriod = 100,
            RequestsUsed = 0,
            PeriodStartDate = DateTimeOffset.UtcNow,
            PeriodEndDate = DateTimeOffset.UtcNow.AddDays(30),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.AIQuotas.Add(quota);
        await db.SaveChangesAsync();

        // Set global AI kill switch to disabled
        var globalSetting = new GlobalSetting
        {
            Key = "AI.Enabled",
            Value = "false",
            Description = "Global AI kill switch",
            Category = "FeatureFlags",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.GlobalSettings.Add(globalSetting);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: Try to use AI feature
        var request = new { description = "Test task description" };
        var response = await _client.PostAsJsonAsync("/api/v1/Agent/improve-task", request);

        // Assert: Should return 403 Forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var errorResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Detail.Should().Contain("system-wide");
    }

    private async Task<Organization> EnsureOrganizationExistsAsync(AppDbContext db)
    {
        var org = await db.Organizations.FirstOrDefaultAsync();
        if (org == null)
        {
            org = new Organization { Name = "Test Organization", Code = "test-org", CreatedAt = DateTimeOffset.UtcNow };
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
        var configuration = _factory.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var tokenService = new IntelliPM.Infrastructure.Identity.JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, new List<string>());
    }

    private class ProblemDetails
    {
        public string? Detail { get; set; }
        public string? Title { get; set; }
        public int? Status { get; set; }
    }
}
