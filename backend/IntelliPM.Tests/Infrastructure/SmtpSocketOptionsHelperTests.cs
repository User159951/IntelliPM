using Xunit;
using FluentAssertions;
using MailKit.Security;
using IntelliPM.Infrastructure.Services;
using Moq;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Tests.Infrastructure;

public class SmtpSocketOptionsHelperTests
{
    [Fact]
    public void GetSecureSocketOptions_Port465_ReturnsSslOnConnect()
    {
        // Arrange
        int port = 465;

        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port);

        // Assert
        result.Should().Be(SecureSocketOptions.SslOnConnect);
    }

    [Fact]
    public void GetSecureSocketOptions_Port587_ReturnsStartTls()
    {
        // Arrange
        int port = 587;

        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port);

        // Assert
        result.Should().Be(SecureSocketOptions.StartTls);
    }

    [Theory]
    [InlineData(25)]
    [InlineData(2525)]
    [InlineData(993)]
    [InlineData(143)]
    [InlineData(80)]
    public void GetSecureSocketOptions_OtherPort_ReturnsAuto(int port)
    {
        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port);

        // Assert
        result.Should().Be(SecureSocketOptions.Auto);
    }

    [Theory]
    [InlineData("SslOnConnect", SecureSocketOptions.SslOnConnect)]
    [InlineData("sslOnConnect", SecureSocketOptions.SslOnConnect)] // Case-insensitive
    [InlineData("SSLONCONNECT", SecureSocketOptions.SslOnConnect)] // Case-insensitive
    [InlineData("StartTls", SecureSocketOptions.StartTls)]
    [InlineData("starttls", SecureSocketOptions.StartTls)] // Case-insensitive
    [InlineData("Auto", SecureSocketOptions.Auto)]
    [InlineData("auto", SecureSocketOptions.Auto)] // Case-insensitive
    [InlineData("None", SecureSocketOptions.None)]
    [InlineData("StartTlsWhenAvailable", SecureSocketOptions.StartTlsWhenAvailable)]
    public void GetSecureSocketOptions_ValidOverride_HonorsOverride(string overrideValue, SecureSocketOptions expected)
    {
        // Arrange
        int port = 587; // Default would be StartTls, but override should take precedence

        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port, overrideValue);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetSecureSocketOptions_OverrideWithWhitespace_TrimsAndHonorsOverride()
    {
        // Arrange
        int port = 465; // Default would be SslOnConnect
        string overrideValue = "  StartTls  "; // With whitespace

        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port, overrideValue);

        // Assert
        result.Should().Be(SecureSocketOptions.StartTls);
    }

    [Theory]
    [InlineData("InvalidOption")]
    [InlineData("Ssl")]
    [InlineData("Tls")]
    [InlineData("")]
    [InlineData("   ")]
    public void GetSecureSocketOptions_InvalidOverride_FallsBackToPortBasedLogic(string invalidOverride)
    {
        // Arrange
        int port = 587; // Default would be StartTls
        var mockLogger = new Mock<ILogger>();

        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port, invalidOverride, mockLogger.Object);

        // Assert
        result.Should().Be(SecureSocketOptions.StartTls); // Falls back to port-based logic
    }

    [Fact]
    public void GetSecureSocketOptions_InvalidOverride_LogsWarning()
    {
        // Arrange
        int port = 587;
        string invalidOverride = "InvalidOption";
        var mockLogger = new Mock<ILogger>();

        // Act
        SmtpSocketOptionsHelper.GetSecureSocketOptions(port, invalidOverride, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid Email:SecureSocketOptions")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetSecureSocketOptions_ValidOverride_LogsDebug()
    {
        // Arrange
        int port = 587;
        string overrideValue = "SslOnConnect";
        var mockLogger = new Mock<ILogger>();

        // Act
        SmtpSocketOptionsHelper.GetSecureSocketOptions(port, overrideValue, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using SecureSocketOptions override")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetSecureSocketOptions_NoOverride_LogsDebugWithPortBasedLogic()
    {
        // Arrange
        int port = 587;
        var mockLogger = new Mock<ILogger>();

        // Act
        SmtpSocketOptionsHelper.GetSecureSocketOptions(port, null, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using port-based SecureSocketOptions")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetSecureSocketOptions_NullOverride_UsesPortBasedLogic()
    {
        // Arrange
        int port = 465;

        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port, null);

        // Assert
        result.Should().Be(SecureSocketOptions.SslOnConnect);
    }

    [Fact]
    public void GetSecureSocketOptions_OverrideTakesPrecedenceOverPort()
    {
        // Arrange
        int port = 465; // Would normally return SslOnConnect
        string overrideValue = "StartTls";

        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port, overrideValue);

        // Assert
        result.Should().Be(SecureSocketOptions.StartTls); // Override takes precedence
    }

    [Fact]
    public void GetSecureSocketOptions_UnsupportedEnumValue_FallsBackToPortBasedLogic()
    {
        // Arrange
        int port = 587;
        // Try to use an enum value that exists but is not in the accepted list
        // Note: This test verifies that even if Enum.TryParse succeeds, 
        // we still validate against the accepted values list
        string overrideValue = "SecureSocketOptions.SslOnConnect"; // Invalid format (includes namespace)

        // Act
        var result = SmtpSocketOptionsHelper.GetSecureSocketOptions(port, overrideValue);

        // Assert
        result.Should().Be(SecureSocketOptions.StartTls); // Falls back to port-based logic
    }
}

