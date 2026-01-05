using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Admin.Dashboard.Queries;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for admin dashboard statistics (Admin only).
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IMediator mediator,
        ILogger<DashboardController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get admin dashboard statistics including user counts, project counts, growth metrics, and recent activities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics</returns>
    /// <response code="200">Dashboard statistics retrieved successfully</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("stats")]
    [RequirePermission("admin.panel.view")]
    [ProducesResponseType(typeof(AdminDashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAdminDashboardStatsQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin dashboard statistics: {Message}\n{StackTrace}", 
                ex.Message, ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving dashboard statistics." });
        }
    }
}

