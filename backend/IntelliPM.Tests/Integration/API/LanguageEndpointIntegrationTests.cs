using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
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
/// Integration tests for language endpoints:
/// - GET /api/v1/Settings/language
/// - PUT /api/v1/Settings/language
/// - Fallback chain behavior
/// </summary>
public class LanguageEndpointIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LanguageEndpointIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async SystemTask GetLanguage_WhenUserHasPreference_ReturnsUserPreference()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Language Test Org",
            Code = "lang-test-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "lang@test.com",
            Username = "languser",
            FirstName = "Lang",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            PreferredLanguage = "fr",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Settings/language");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LanguageResponse>();
        result.Should().NotBeNull();
        result!.Language.Should().Be("fr");
    }

    [Fact]
    public async SystemTask GetLanguage_WhenNoUserPreference_UsesOrganizationDefault()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Lang Org Default",
            Code = "lang-org-default",
            DefaultLanguage = "ar",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "orglang@test.com",
            Username = "orglanguser",
            FirstName = "Org",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            PreferredLanguage = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Settings/language");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LanguageResponse>();
        result.Should().NotBeNull();
        result!.Language.Should().Be("ar");
    }

    [Fact]
    public async SystemTask GetLanguage_WhenNoPreference_UsesAcceptLanguageHeader()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Browser Lang Org",
            Code = "browser-lang-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "browser@test.com",
            Username = "browseruser",
            FirstName = "Browser",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            PreferredLanguage = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _client.DefaultRequestHeaders.Add("Accept-Language", "fr-FR,fr;q=0.9");

        // Act
        var response = await _client.GetAsync("/api/v1/Settings/language");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LanguageResponse>();
        result.Should().NotBeNull();
        result!.Language.Should().Be("fr");
    }

    [Fact]
    public async SystemTask GetLanguage_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/v1/Settings/language");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async SystemTask UpdateLanguage_WithValidLanguage_UpdatesUserPreference()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Update Lang Org",
            Code = "update-lang-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "update@test.com",
            Username = "updateuser",
            FirstName = "Update",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            PreferredLanguage = "en",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var updateRequest = new UpdateLanguageRequest { Language = "ar" };
        var response = await _client.PutAsJsonAsync("/api/v1/Settings/language", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LanguageResponse>();
        result.Should().NotBeNull();
        result!.Language.Should().Be("ar");

        // Verify database was updated
        var updatedUser = await db.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.PreferredLanguage.Should().Be("ar");
    }

    [Fact]
    public async SystemTask UpdateLanguage_WithUnsupportedLanguage_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var organization = new Organization
        {
            Name = "Invalid Lang Org",
            Code = "invalid-lang-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "invalid@test.com",
            Username = "invaliduser",
            FirstName = "Invalid",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = organization.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var updateRequest = new UpdateLanguageRequest { Language = "de" };
        var response = await _client.PutAsJsonAsync("/api/v1/Settings/language", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async SystemTask UpdateLanguage_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var updateRequest = new UpdateLanguageRequest { Language = "fr" };
        var response = await _client.PutAsJsonAsync("/api/v1/Settings/language", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private string GenerateJwtToken(int userId, string username, string email)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, new List<string>());
    }

    private class LanguageResponse
    {
        public string Language { get; set; } = string.Empty;
    }

    private class UpdateLanguageRequest
    {
        public string Language { get; set; } = string.Empty;
    }
}
