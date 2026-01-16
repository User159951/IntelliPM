using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Identity.Commands;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Tests.Application.Identity;

public class LoginCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();

        var loginResult = new LoginResult(
            UserId: 1,
            Username: "john",
            Email: "john@test.com",
            GlobalRole: GlobalRole.User.ToString(),
            AccessToken: "accessToken",
            RefreshToken: "refreshToken"
        );

        mockAuthService.Setup(s => s.LoginAsync("john", "password123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        var handler = new LoginCommandHandler(mockAuthService.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("john", "password123"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(1);
        result.Username.Should().Be("john");
        result.Email.Should().Be("john@test.com");
        result.Roles.Should().Contain(GlobalRole.User.ToString());
        result.AccessToken.Should().Be("accessToken");
        result.RefreshToken.Should().Be("refreshToken");
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();

        mockAuthService.Setup(s => s.LoginAsync("john", "wrong", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedException("Invalid credentials"));

        var handler = new LoginCommandHandler(mockAuthService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("john", "wrong"), CancellationToken.None));
    }
}

