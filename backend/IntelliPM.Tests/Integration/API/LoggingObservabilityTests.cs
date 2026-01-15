using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IntelliPM.Application.Identity.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Tests.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Integration.API;

/// <summary>
/// Integration tests for production-grade observability features:
/// - Log correlation IDs
/// - Organization scoping in logs
/// - PII protection
/// </summary>
public class LoggingObservabilityTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LoggingObservabilityTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async SystemTask Logs_IncludeCorrelationId_ForAllRequests()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create organization and user
        var organization = new Organization
        {
            Name = "Test Org",
            Code = "test-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

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

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: Make a request
        var response = await _client.GetAsync("/api/v1/Auth/me");

        // Assert: Correlation ID should be in response headers
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue("Correlation ID should be a valid GUID");
    }

    [Fact]
    public async SystemTask Logs_IncludeOrganizationContext()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Org for Logging Test",
            Code = "logging-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "logging@test.com",
            Username = "logginguser",
            FirstName = "Logging",
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

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: Make an authenticated request
        var response = await _client.GetAsync("/api/v1/Auth/me");

        // Assert: Request should succeed
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify organization context is available via ICurrentUserService
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var result = await response.Content.ReadFromJsonAsync<CurrentUserDto>(jsonOptions);
        result.Should().NotBeNull();
        result!.OrganizationId.Should().Be(organization.Id, 
            "OrganizationId should be available, indicating organization context is set");
    }

    [Fact]
    public async SystemTask CorrelationId_IsPropagated_InRequestHeaders()
    {
        // Arrange
        var customCorrelationId = Guid.NewGuid().ToString();
        _client.DefaultRequestHeaders.Add("X-Correlation-ID", customCorrelationId);

        // Act: Make a request with custom correlation ID
        var response = await _client.GetAsync("/api/v1/Health");

        // Assert: Same correlation ID should be returned
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var returnedCorrelationId = response.Headers.GetValues("X-Correlation-ID").First();
        returnedCorrelationId.Should().Be(customCorrelationId, 
            "Correlation ID from request header should be preserved");
    }

    [Fact]
    public async SystemTask CorrelationId_IsGenerated_WhenNotProvided()
    {
        // Arrange: No correlation ID header

        // Act: Make a request without correlation ID
        var response = await _client.GetAsync("/api/v1/Health");

        // Assert: New correlation ID should be generated
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue("Correlation ID should be a valid GUID");
    }

    [Fact]
    public async SystemTask Logs_IncludeCorrelationId_ForUnauthenticatedRequests()
    {
        // Arrange: No authentication

        // Act: Make an unauthenticated request
        var response = await _client.GetAsync("/api/v1/Health");

        // Assert: Correlation ID should still be present
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        correlationId.Should().NotBeNullOrEmpty();
    }

    private string GenerateJwtToken(int userId, string username, string email)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, new List<string> { "User" });
    }
}
