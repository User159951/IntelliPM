using FluentAssertions;
using IntelliPM.Infrastructure.Utilities;
using Xunit;

namespace IntelliPM.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for PII redaction utility
/// </summary>
public class PiiRedactorTests
{
    [Fact]
    public void Redact_EmailAddress_ShouldBeRedacted()
    {
        // Arrange
        var input = "Contact user@example.com for details";

        // Act
        var result = PiiRedactor.Redact(input);

        // Assert
        result.Should().Contain("[REDACTED_EMAIL]");
        result.Should().NotContain("user@example.com");
    }

    [Fact]
    public void Redact_PasswordField_ShouldBeRedacted()
    {
        // Arrange
        var password = "MySecretPassword123!";

        // Act
        var result = PiiRedactor.Redact(password, "password");

        // Assert
        result.Should().Be("[REDACTED]");
    }

    [Fact]
    public void Redact_TokenField_ShouldBeRedacted()
    {
        // Arrange
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ";

        // Act
        var result = PiiRedactor.Redact(token, "access_token");

        // Assert
        result.Should().Be("[REDACTED]");
    }

    [Fact]
    public void Redact_CreditCard_ShouldBeRedacted()
    {
        // Arrange
        var input = "Card number: 4532-1234-5678-9010";

        // Act
        var result = PiiRedactor.Redact(input);

        // Assert
        result.Should().Contain("[REDACTED_CC]");
        result.Should().NotContain("4532-1234-5678-9010");
    }

    [Fact]
    public void Redact_SSN_ShouldBeRedacted()
    {
        // Arrange
        var input = "SSN: 123-45-6789";

        // Act
        var result = PiiRedactor.Redact(input);

        // Assert
        result.Should().Contain("[REDACTED_SSN]");
        result.Should().NotContain("123-45-6789");
    }

    [Fact]
    public void Redact_NonSensitiveData_ShouldNotBeRedacted()
    {
        // Arrange
        var input = "This is a normal message without sensitive data";

        // Act
        var result = PiiRedactor.Redact(input);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void Redact_NullValue_ShouldReturnEmpty()
    {
        // Act
        var result = PiiRedactor.Redact(null);

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Redact_EmptyString_ShouldReturnEmpty()
    {
        // Act
        var result = PiiRedactor.Redact(string.Empty);

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void IsSensitiveField_PasswordField_ShouldReturnTrue()
    {
        // Act
        var result = PiiRedactor.IsSensitiveField("password");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSensitiveField_TokenField_ShouldReturnTrue()
    {
        // Act
        var result = PiiRedactor.IsSensitiveField("access_token");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSensitiveField_RegularField_ShouldReturnFalse()
    {
        // Act
        var result = PiiRedactor.IsSensitiveField("username");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SanitizeObject_WithSensitiveAttribute_ShouldRedact()
    {
        // Arrange
        var obj = new TestDto
        {
            Username = "testuser",
            Password = "secret123",
            Email = "test@example.com"
        };

        // Act
        var result = PiiRedactor.SanitizeObject(obj);

        // Assert
        result["Password"].Should().Be("[REDACTED]");
        result["Username"].Should().Be("testuser");
        result["Email"].Should().Be("[REDACTED_EMAIL]");
    }

    [Fact]
    public void SanitizeDictionary_WithSensitiveKeys_ShouldRedact()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            { "username", "testuser" },
            { "password", "secret123" },
            { "email", "test@example.com" }
        };

        // Act
        var result = PiiRedactor.SanitizeDictionary(dict);

        // Assert
        result["password"].Should().Be("[REDACTED]");
        result["username"].Should().Be("testuser");
        result["email"].Should().Be("[REDACTED_EMAIL]");
    }

    private class TestDto
    {
        public string Username { get; set; } = string.Empty;

        [Sensitive]
        public string Password { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
