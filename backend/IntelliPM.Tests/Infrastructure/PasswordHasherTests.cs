using Xunit;
using FluentAssertions;
using IntelliPM.Infrastructure.Identity;

namespace IntelliPM.Tests.Infrastructure;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_GeneratesHashAndSalt()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        salt.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(salt);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "TestPassword123!";
        var (hash, salt) = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "TestPassword123!";
        var (hash, salt) = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword("WrongPassword", hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_SamePassword_GeneratesDifferentHashes()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "TestPassword123!";

        // Act
        var (hash1, salt1) = hasher.HashPassword(password);
        var (hash2, salt2) = hasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
        salt1.Should().NotBe(salt2);
    }
}

