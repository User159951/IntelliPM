using FluentAssertions;
using IntelliPM.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace IntelliPM.Tests.API;

/// <summary>
/// Tests for HealthApiController.CheckApiHealth endpoint
/// </summary>
public class HealthApiEndpoint_Tests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<HealthApiController>> _loggerMock;
    private readonly HealthApiController _controller;

    public HealthApiEndpoint_Tests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<HealthApiController>>();

        // Setup default configuration
        _configurationMock.Setup(c => c["BaseUrl"]).Returns("http://localhost:5001");

        _controller = new HealthApiController(
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    private Mock<HttpMessageHandler> SetupHttpMessageHandler(
        Dictionary<string, HttpStatusCode> endpointResponses)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        // Setup specific endpoints first (more specific matches come first in Moq)
        foreach (var (endpoint, statusCode) in endpointResponses)
        {
            var endpointToMatch = endpoint; // Capture for closure
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri != null && 
                        req.RequestUri.AbsolutePath == endpointToMatch),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(string.Empty)
                });
        }
        
        // Setup default handler for any unmatched requests (comes after specific ones)
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(string.Empty)
            });

        return handlerMock;
    }

    [Fact]
    public async Task CheckApiHealth_AllEndpointsHealthy_ReturnsHealthyStatus()
    {
        // Arrange
        var endpointResponses = new Dictionary<string, HttpStatusCode>
        {
            { "/api/v1/Auth/me", HttpStatusCode.Unauthorized },
            { "/api/v1/Projects", HttpStatusCode.Unauthorized },
            { "/api/health", HttpStatusCode.OK },
            { "/swagger/index.html", HttpStatusCode.OK }
        };

        var handlerMock = SetupHttpMessageHandler(endpointResponses);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _controller.CheckApiHealth(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();

        var response = okResult!.Value as ApiHealthResponse;
        response.Should().NotBeNull();
        response!.Status.Should().Be("Healthy");
        response.Checks.Should().HaveCount(4);
        response.Checks.Should().OnlyContain(c => c.Status == "OK");
    }

    [Fact]
    public async Task CheckApiHealth_AuthEndpointReturnsWrongStatus_ReturnsDegraded()
    {
        // Arrange
        var endpointResponses = new Dictionary<string, HttpStatusCode>
        {
            { "/api/v1/Auth/me", HttpStatusCode.OK }, // Wrong! Should be 401
            { "/api/v1/Projects", HttpStatusCode.Unauthorized },
            { "/api/health", HttpStatusCode.OK },
            { "/swagger/index.html", HttpStatusCode.OK }
        };

        var handlerMock = SetupHttpMessageHandler(endpointResponses);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _controller.CheckApiHealth(CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiHealthResponse;

        response!.Status.Should().Be("Degraded");
        response.Checks.Should().Contain(c => c.Status == "Failed");
    }

    [Fact]
    public async Task CheckApiHealth_HttpClientThrowsException_ReturnsUnhealthy()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _controller.CheckApiHealth(CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiHealthResponse;

        response!.Status.Should().Be("Unhealthy");
        response.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckApiHealth_ReturnsTimestamp()
    {
        // Arrange
        var endpointResponses = new Dictionary<string, HttpStatusCode>
        {
            { "/api/v1/Auth/me", HttpStatusCode.Unauthorized },
            { "/api/v1/Projects", HttpStatusCode.Unauthorized },
            { "/api/health", HttpStatusCode.OK },
            { "/swagger/index.html", HttpStatusCode.OK }
        };

        var handlerMock = SetupHttpMessageHandler(endpointResponses);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var beforeCall = DateTimeOffset.UtcNow;

        // Act
        var result = await _controller.CheckApiHealth(CancellationToken.None);

        var afterCall = DateTimeOffset.UtcNow;

        // Assert
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiHealthResponse;

        response!.Timestamp.Should().BeAfter(beforeCall.AddSeconds(-1));
        response.Timestamp.Should().BeBefore(afterCall.AddSeconds(1));
    }

    [Fact]
    public async Task CheckApiHealth_VerifiesExpectedAndActualStatusCodes()
    {
        // Arrange
        var endpointResponses = new Dictionary<string, HttpStatusCode>
        {
            { "/api/v1/Auth/me", HttpStatusCode.Unauthorized },
            { "/api/v1/Projects", HttpStatusCode.Unauthorized },
            { "/api/health", HttpStatusCode.OK },
            { "/swagger/index.html", HttpStatusCode.OK }
        };

        var handlerMock = SetupHttpMessageHandler(endpointResponses);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _controller.CheckApiHealth(CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiHealthResponse;

        var authCheck = response!.Checks.First(c => c.Endpoint == "/api/v1/Auth/me");
        authCheck.ExpectedStatus.Should().Be(401);
        authCheck.ActualStatus.Should().Be(401);
        authCheck.Status.Should().Be("OK");
    }

    [Fact]
    public async Task CheckApiHealth_UsesBaseUrlFromConfiguration()
    {
        // Arrange
        var customBaseUrl = "http://custom-url:8080";
        _configurationMock.Setup(c => c["BaseUrl"]).Returns(customBaseUrl);

        var endpointResponses = new Dictionary<string, HttpStatusCode>
        {
            { "/api/v1/Auth/me", HttpStatusCode.Unauthorized },
            { "/api/v1/Projects", HttpStatusCode.Unauthorized },
            { "/api/health", HttpStatusCode.OK },
            { "/swagger/index.html", HttpStatusCode.OK }
        };

        var handlerMock = SetupHttpMessageHandler(endpointResponses);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(customBaseUrl)
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _controller.CheckApiHealth(CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiHealthResponse;

        response!.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task CheckApiHealth_UsesDefaultBaseUrlWhenNotConfigured()
    {
        // Arrange
        _configurationMock.Setup(c => c["BaseUrl"]).Returns((string?)null);

        var endpointResponses = new Dictionary<string, HttpStatusCode>
        {
            { "/api/v1/Auth/me", HttpStatusCode.Unauthorized },
            { "/api/v1/Projects", HttpStatusCode.Unauthorized },
            { "/api/health", HttpStatusCode.OK },
            { "/swagger/index.html", HttpStatusCode.OK }
        };

        var handlerMock = SetupHttpMessageHandler(endpointResponses);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _controller.CheckApiHealth(CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiHealthResponse;

        response!.Status.Should().Be("Healthy");
    }
}

