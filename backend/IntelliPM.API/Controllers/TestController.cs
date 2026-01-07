#if DEBUG
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace IntelliPM.API.Controllers;

/// <summary>
/// ⚠️ WARNING: DEBUG-ONLY CONTROLLER ⚠️
/// 
/// This controller is ONLY available in DEBUG builds and will be COMPLETELY DISABLED in Release builds.
/// 
/// DO NOT use this controller in production. All endpoints in this controller are excluded from Release builds
/// via #if DEBUG preprocessor directive. Any code referencing this controller will fail to compile in Release mode.
/// 
/// This controller is intended for development and debugging purposes only, such as:
/// - Testing error tracking integrations (e.g., Sentry)
/// - Development-only diagnostic endpoints
/// - Temporary debugging utilities
/// 
/// Security Note: Even though this controller is disabled in Release builds, ensure that any test endpoints
/// do not expose sensitive data or perform destructive operations.
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

