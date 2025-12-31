using FluentAssertions;
using IntelliPM.API.Controllers;
using IntelliPM.Application.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace IntelliPM.Tests.API;

/// <summary>
/// Tests for NotificationsController.GetUnreadCount endpoint
/// </summary>
public class NotificationsController_GetUnreadCount_Tests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<NotificationsController>> _loggerMock;
    private readonly NotificationsController _controller;

    public NotificationsController_GetUnreadCount_Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<NotificationsController>>();
        _controller = new NotificationsController(_mediatorMock.Object, _loggerMock.Object);
    }

    private void SetupAuthenticatedUser(int userId, int organizationId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("organizationId", organizationId.ToString()),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetUnreadCount_WithAuthenticatedUser_ReturnsOkWithCorrectCount()
    {
        // Arrange
        const int userId = 1;
        const int organizationId = 100;
        const int expectedUnreadCount = 5;

        SetupAuthenticatedUser(userId, organizationId);

        var expectedResponse = new GetUnreadNotificationCountResponse(expectedUnreadCount);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUnreadNotificationCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetUnreadCount(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        var response = okResult.Value as GetUnreadNotificationCountResponse;
        response.Should().NotBeNull();
        response!.UnreadCount.Should().Be(expectedUnreadCount);

        // Verify MediatR was called with correct query
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetUnreadNotificationCountQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUnreadCount_WithZeroUnreadNotifications_ReturnsOkWithZeroCount()
    {
        // Arrange
        const int userId = 2;
        const int organizationId = 100;

        SetupAuthenticatedUser(userId, organizationId);

        var expectedResponse = new GetUnreadNotificationCountResponse(UnreadCount: 0);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUnreadNotificationCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetUnreadCount(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as GetUnreadNotificationCountResponse;
        response!.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task GetUnreadCount_WithoutAuthentication_ReturnsOkButMayFailInRealScenario()
    {
        // Arrange
        // Don't call SetupAuthenticatedUser - simulate unauthenticated request
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Note: The [Authorize] attribute is handled by middleware in real app
        // In unit tests, we test the controller behavior assuming auth middleware ran
        // For integration tests, use WebApplicationFactory to test full pipeline
        
        // This test verifies the controller doesn't crash without auth
        // Real 401 is handled by ASP.NET Core authentication middleware
        var expectedResponse = new GetUnreadNotificationCountResponse(0);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUnreadNotificationCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetUnreadCount(CancellationToken.None);
        
        // Assert - Controller should still work (auth is middleware concern)
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUnreadCount_WhenMediatorThrowsException_ReturnsProblemResult()
    {
        // Arrange
        const int userId = 3;
        const int organizationId = 100;

        SetupAuthenticatedUser(userId, organizationId);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUnreadNotificationCountQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _controller.GetUnreadCount(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetUnreadCount_VerifyQuerySentToMediator()
    {
        // Arrange
        SetupAuthenticatedUser(userId: 10, organizationId: 200);

        var expectedResponse = new GetUnreadNotificationCountResponse(3);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUnreadNotificationCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _controller.GetUnreadCount(CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetUnreadNotificationCountQuery>(q => q != null),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "GetUnreadCount should send GetUnreadNotificationCountQuery to MediatR");
    }
}

