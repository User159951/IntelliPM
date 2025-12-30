using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Admin.AuditLogs.Queries;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for viewing audit logs (Admin only).
/// </summary>
[ApiController]
[Route("api/admin/audit-logs")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IMediator mediator,
        ILogger<AuditLogsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get audit logs with filters and pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="action">Filter by action</param>
    /// <param name="entityType">Filter by entity type</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of audit logs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1)
            {
                return BadRequest(new { error = "Page must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "PageSize must be between 1 and 100" });
            }

            var query = new GetAuditLogsQuery(
                page,
                pageSize,
                action,
                entityType,
                userId,
                startDate,
                endDate);

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to access audit logs.");
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
        }
    }
}

