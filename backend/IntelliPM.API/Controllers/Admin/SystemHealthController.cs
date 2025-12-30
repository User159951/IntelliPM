using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Admin.SystemHealth.Queries;
using IntelliPM.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for viewing system health metrics (Admin only).
/// </summary>
[ApiController]
[Route("api/admin/system-health")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class SystemHealthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SystemHealthController> _logger;

    public SystemHealthController(
        IMediator mediator,
        ILogger<SystemHealthController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get current system health metrics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System health metrics</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SystemHealthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSystemHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetSystemHealthQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to access system health.");
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system health.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
        }
    }
}

