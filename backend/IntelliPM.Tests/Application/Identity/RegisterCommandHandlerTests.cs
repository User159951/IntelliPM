using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Identity.Commands;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using DomainTask = IntelliPM.Domain.Entities.Task;

namespace IntelliPM.Tests.Application.Identity;

public class RegisterCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_WithValidData_ReturnsRegisterResponse()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockTokenService = new Mock<ITokenService>();
        var mockRefreshTokenRepo = new Mock<IRepository<RefreshToken>>();

        var userId = 1;
        mockAuthService.Setup(s => s.RegisterAsync(
            "john", "john@test.com", "password123", "John", "Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        mockTokenService.Setup(t => t.GenerateAccessToken(
            userId, "john", "john@test.com", It.IsAny<List<string>>()))
            .Returns("accessToken");

        mockTokenService.Setup(t => t.GenerateRefreshToken())
            .Returns("refreshToken");

        mockUnitOfWork.Setup(u => u.Repository<RefreshToken>())
            .Returns(mockRefreshTokenRepo.Object);

        var handler = new RegisterCommandHandler(mockAuthService.Object, mockUnitOfWork.Object, mockTokenService.Object);

        // Act
        var result = await handler.Handle(
            new RegisterCommand("john", "john@test.com", "password123", "John", "Doe"),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Username.Should().Be("john");
        result.Email.Should().Be("john@test.com");
        result.AccessToken.Should().Be("accessToken");
        result.RefreshToken.Should().Be("refreshToken");

        mockRefreshTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

