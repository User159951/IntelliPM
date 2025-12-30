using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Identity.Commands;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using DomainTask = IntelliPM.Domain.Entities.Task;

namespace IntelliPM.Tests.Application.Identity;

public class LoginCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockUserRepo = new Mock<IRepository<User>>();

        var user = new User
        {
            Id = 1,
            Username = "john",
            Email = "john@test.com"
        };

        mockAuthService.Setup(s => s.LoginAsync("john", "password123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("accessToken", "refreshToken"));

        mockUserRepo.Setup(r => r.Query())
            .Returns(new List<User> { user }.AsQueryable());

        mockUnitOfWork.Setup(u => u.Repository<User>())
            .Returns(mockUserRepo.Object);

        var handler = new LoginCommandHandler(mockAuthService.Object, mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("john", "password123"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("john");
        result.AccessToken.Should().Be("accessToken");
        result.RefreshToken.Should().Be("refreshToken");
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        mockAuthService.Setup(s => s.LoginAsync("john", "wrong", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedException("Invalid credentials"));

        var handler = new LoginCommandHandler(mockAuthService.Object, mockUnitOfWork.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("john", "wrong"), CancellationToken.None));
    }
}

