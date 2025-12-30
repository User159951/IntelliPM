using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using IntelliPM.Infrastructure.Identity;
using System.Security.Claims;

namespace IntelliPM.Tests.Infrastructure;

public class JwtTokenServiceTests
{
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:SecretKey", "YourSecureSecretKeyOfAt32CharactersMinimumForTesting!"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidToken()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var userId = 1;
        var username = "testuser";
        var email = "test@test.com";
        var roles = new List<string> { "Developer" };

        // Act
        var token = service.GenerateAccessToken(userId, username, email, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentTokens()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);

        // Act
        var token1 = service.GenerateRefreshToken();
        var token2 = service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var token = service.GenerateAccessToken(1, "testuser", "test@test.com", new List<string> { "Developer" });

        // Act
        var isValid = service.ValidateToken(token, out var principal);

        // Assert
        isValid.Should().BeTrue();
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.Name)?.Value.Should().Be("testuser");
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = service.ValidateToken(invalidToken, out var principal);

        // Assert
        isValid.Should().BeFalse();
        principal.Should().BeNull();
    }
}

