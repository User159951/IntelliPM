#if DEBUG
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Test controller for development and debugging purposes
/// This controller is only available in DEBUG mode and will be excluded from Release builds.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to verify Sentry integration
    /// </summary>
    /// <returns>Never returns (throws exception)</returns>
    [HttpGet("sentry")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult TestSentry()
    {
        _logger.LogWarning("Test Sentry endpoint called - throwing exception");
        throw new Exception("Test Sentry integration - This is a test exception to verify error tracking");
    }
}
#endif

