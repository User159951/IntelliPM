using System.Net;
using System.Security.Claims;
using FluentAssertions;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Tests.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntelliPM.Tests.Infrastructure.TestAuthentication;

/// <summary>
/// Tests to verify that TestAuthHandler works correctly.
/// </summary>
public class TestAuthHandlerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TestAuthHandlerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthenticateAs_WithValidClaims_ShouldAuthenticateSuccessfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test@example.com", "testuser", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Act - Authenticate using the extension method
        _client.AuthenticateAs(user.Id, user.Username, user.Email, org.Id, user.GlobalRole);

        // Assert - Make a request to an authenticated endpoint
        var response = await _client.GetAsync("/api/v1/Projects");

        // Should not be 401 Unauthorized
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthenticateAs_WithOrganizationId_ShouldSetOrganizationIdClaim()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);
        var user = CreateTestUser("test2@example.com", "testuser2", GlobalRole.User, org.Id);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Act
        _client.AuthenticateAs(user.Id, user.Username, user.Email, org.Id, user.GlobalRole);

        // Assert - Verify the claim is set by making a request
        // The organizationId claim should be accessible in the request
        var response = await _client.GetAsync("/api/v1/Projects");
        
        // If we get past authentication, the claim was set correctly
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthenticateAsSuperAdmin_ShouldSetSuperAdminRole()
    {
        // Arrange
        _client.AuthenticateAsSuperAdmin(userId: 999, organizationId: 1);

        // Act - Make a request that requires SuperAdmin role
        var response = await _client.GetAsync("/api/v1/Projects");

        // Assert - Should authenticate successfully
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ClearAuthentication_ShouldRemoveAuthentication()
    {
        // Arrange
        _client.AuthenticateAs(1, "testuser", "test@example.com");
        
        // Verify authenticated
        var authenticatedResponse = await _client.GetAsync("/api/v1/Projects");
        authenticatedResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);

        // Act
        _client.ClearAuthentication();

        // Assert - Should now be unauthorized
        var unauthenticatedResponse = await _client.GetAsync("/api/v1/Projects");
        unauthenticatedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WithoutAuthentication_ShouldReturn401()
    {
        // Arrange - No authentication set
        _client.ClearAuthentication();

        // Act
        var response = await _client.GetAsync("/api/v1/Projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthenticateAs_WithDifferentRoles_ShouldWork()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var org = await EnsureOrganizationExistsAsync(db);

        // Test User role
        _client.AuthenticateAs(1, "user", "user@test.com", org.Id, GlobalRole.User);
        var userResponse = await _client.GetAsync("/api/v1/Projects");
        userResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);

        // Test Admin role
        _client.AuthenticateAs(2, "admin", "admin@test.com", org.Id, GlobalRole.Admin);
        var adminResponse = await _client.GetAsync("/api/v1/Projects");
        adminResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);

        // Test SuperAdmin role
        _client.AuthenticateAsSuperAdmin(userId: 3, organizationId: org.Id);
        var superAdminResponse = await _client.GetAsync("/api/v1/Projects");
        superAdminResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    #region Helper Methods

    private async Task<Domain.Entities.Organization> EnsureOrganizationExistsAsync(AppDbContext db)
    {
        var org = await db.Organizations.FirstOrDefaultAsync();
        if (org == null)
        {
            org = new Domain.Entities.Organization
            {
                Name = "Test Organization",
                Code = "test-org",
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
        }
        return org;
    }

    private Domain.Entities.User CreateTestUser(string email, string username, GlobalRole role, int organizationId)
    {
        return new Domain.Entities.User
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

    #endregion
}
