using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System.Net;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Controller for API health smoke tests
/// Exposes versioned endpoint at /api/v1/health/api
/// Note: This endpoint is versioned for consistency with other API endpoints.
/// Monitoring tools should be updated to use the new versioned route.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/health")]
[ApiVersion("1.0")]
public class HealthApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthApiController> _logger;

    public HealthApiController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<HealthApiController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Performs smoke tests on critical API endpoints
    /// Tests routing, authentication, and basic endpoint availability
    /// </summary>
    /// <returns>Health check results for API endpoints</returns>
    [HttpGet("api")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiHealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckApiHealth(CancellationToken cancellationToken)
    {
        var checks = new List<EndpointCheck>();
        var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5001";

        try
        {
            // Test 1: Auth endpoint (should return 401 without auth)
            checks.Add(await CheckEndpointAsync(
                "/api/v1/Auth/me",
                HttpMethod.Get,
                baseUrl,
                expectedStatusCode: HttpStatusCode.Unauthorized,
                description: "Authentication endpoint routing",
                cancellationToken));

            // Test 2: Projects endpoint (should return 401 without auth)
            checks.Add(await CheckEndpointAsync(
                "/api/v1/Projects",
                HttpMethod.Get,
                baseUrl,
                expectedStatusCode: HttpStatusCode.Unauthorized,
                description: "Projects endpoint routing",
                cancellationToken));

            // Test 3: Health endpoint (should return 200 - public)
            checks.Add(await CheckEndpointAsync(
                "/api/v1/health",
                HttpMethod.Get,
                baseUrl,
                expectedStatusCode: HttpStatusCode.OK,
                description: "Public health endpoint",
                cancellationToken));

            // Test 4: Swagger endpoint (should return 200 - public)
            checks.Add(await CheckEndpointAsync(
                "/swagger/index.html",
                HttpMethod.Get,
                baseUrl,
                expectedStatusCode: HttpStatusCode.OK,
                description: "Swagger documentation",
                cancellationToken));

            var overallStatus = checks.All(c => c.Status == "OK") ? "Healthy" : "Degraded";

            return Ok(new ApiHealthResponse
            {
                Status = overallStatus,
                Checks = checks,
                Timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing API health checks");

            return Ok(new ApiHealthResponse
            {
                Status = "Unhealthy",
                Checks = checks,
                Error = ex.Message,
                Timestamp = DateTimeOffset.UtcNow
            });
        }
    }

    private async Task<EndpointCheck> CheckEndpointAsync(
        string endpoint,
        HttpMethod method,
        string baseUrl,
        HttpStatusCode expectedStatusCode,
        string description,
        CancellationToken cancellationToken)
    {
        var check = new EndpointCheck
        {
            Endpoint = endpoint,
            Description = description,
            ExpectedStatus = (int)expectedStatusCode
        };

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);

            var request = new HttpRequestMessage(method, endpoint);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await client.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            check.ActualStatus = (int)response.StatusCode;
            check.Status = response.StatusCode == expectedStatusCode ? "OK" : "Failed";
            check.ResponseTime = (int)stopwatch.ElapsedMilliseconds;

            if (check.Status == "Failed")
            {
                check.Message = $"Expected {expectedStatusCode} but got {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            check.Status = "Error";
            check.Message = ex.Message;
            _logger.LogWarning(ex, "Health check failed for endpoint {Endpoint}", endpoint);
        }

        return check;
    }
}

/// <summary>
/// Response model for API health check endpoint
/// </summary>
public class ApiHealthResponse
{
    public string Status { get; set; } = "Unknown";
    public List<EndpointCheck> Checks { get; set; } = new();
    public string? Error { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Individual endpoint check result
/// </summary>
public class EndpointCheck
{
    public string Endpoint { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Unknown";
    public int ExpectedStatus { get; set; }
    public int ActualStatus { get; set; }
    public string? Message { get; set; }
    public int ResponseTime { get; set; }
}

