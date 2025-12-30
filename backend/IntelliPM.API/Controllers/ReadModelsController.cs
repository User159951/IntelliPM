using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Projections.Queries;
using IntelliPM.Application.Common.Models;
using Microsoft.Net.Http.Headers;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Read Models Controller - Optimized queries using read models
/// </summary>
/// <remarks>
/// This controller provides access to denormalized read models that are optimized for query performance.
/// Read models are automatically updated via projection handlers when domain events occur.
/// Use ETags for caching to minimize bandwidth usage.
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class ReadModelsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReadModelsController> _logger;

    public ReadModelsController(
        IMediator mediator,
        ILogger<ReadModelsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get task board read model for a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task board with pre-grouped tasks</returns>
    /// <response code="200">Task board retrieved successfully</response>
    /// <response code="304">Not Modified - Resource hasn't changed (ETag match)</response>
    /// <response code="404">Task board not found for the specified project</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("task-board/{projectId}")]
    [ProducesResponseType(typeof(TaskBoardReadModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTaskBoard(int projectId, CancellationToken ct)
    {
        try
        {
            _logger.LogDebug("Getting task board for project {ProjectId}", projectId);

            var query = new GetTaskBoardQuery { ProjectId = projectId };
            var result = await _mediator.Send(query, ct);

            if (result == null)
            {
                _logger.LogInformation("Task board not found for project {ProjectId}", projectId);
                return NotFound(new { message = $"Task board not found for project {projectId}" });
            }

            // Check If-None-Match header for conditional request
            var etag = $"\"{result.Version}\"";
            if (Request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
            {
                var ifNoneMatch = Request.Headers[HeaderNames.IfNoneMatch].ToString();
                if (ifNoneMatch == etag)
                {
                    _logger.LogDebug("Task board for project {ProjectId} not modified (ETag match)", projectId);
                    return StatusCode(StatusCodes.Status304NotModified);
                }
            }

            // Add ETag and cache headers
            Response.Headers[HeaderNames.ETag] = etag;
            Response.Headers[HeaderNames.CacheControl] = "private, max-age=60";
            Response.Headers[HeaderNames.Expires] = DateTimeOffset.UtcNow.AddSeconds(60).ToString("R");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task board for project {ProjectId}", projectId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving the task board.", message = ex.Message });
        }
    }

    /// <summary>
    /// Get sprint summary read model
    /// </summary>
    /// <param name="sprintId">Sprint ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sprint summary with pre-calculated metrics</returns>
    /// <response code="200">Sprint summary retrieved successfully</response>
    /// <response code="304">Not Modified - Resource hasn't changed (ETag match)</response>
    /// <response code="404">Sprint summary not found for the specified sprint</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("sprint-summary/{sprintId}")]
    [ProducesResponseType(typeof(SprintSummaryReadModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSprintSummary(int sprintId, CancellationToken ct)
    {
        try
        {
            _logger.LogDebug("Getting sprint summary for sprint {SprintId}", sprintId);

            var query = new GetSprintSummaryQuery { SprintId = sprintId };
            var result = await _mediator.Send(query, ct);

            if (result == null)
            {
                _logger.LogInformation("Sprint summary not found for sprint {SprintId}", sprintId);
                return NotFound(new { message = $"Sprint summary not found for sprint {sprintId}" });
            }

            // Check If-None-Match header for conditional request
            var etag = $"\"{result.Version}\"";
            if (Request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
            {
                var ifNoneMatch = Request.Headers[HeaderNames.IfNoneMatch].ToString();
                if (ifNoneMatch == etag)
                {
                    _logger.LogDebug("Sprint summary for sprint {SprintId} not modified (ETag match)", sprintId);
                    return StatusCode(StatusCodes.Status304NotModified);
                }
            }

            // Add ETag and cache headers
            Response.Headers[HeaderNames.ETag] = etag;
            Response.Headers[HeaderNames.CacheControl] = "private, max-age=60";
            Response.Headers[HeaderNames.Expires] = DateTimeOffset.UtcNow.AddSeconds(60).ToString("R");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sprint summary for sprint {SprintId}", sprintId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving the sprint summary.", message = ex.Message });
        }
    }

    /// <summary>
    /// Get project overview read model
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Project overview with aggregated metrics</returns>
    /// <response code="200">Project overview retrieved successfully</response>
    /// <response code="304">Not Modified - Resource hasn't changed (ETag match)</response>
    /// <response code="404">Project overview not found for the specified project</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("project-overview/{projectId}")]
    [ProducesResponseType(typeof(ProjectOverviewReadModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectOverview(int projectId, CancellationToken ct)
    {
        try
        {
            _logger.LogDebug("Getting project overview for project {ProjectId}", projectId);

            var query = new GetProjectOverviewQuery { ProjectId = projectId };
            var result = await _mediator.Send(query, ct);

            if (result == null)
            {
                _logger.LogInformation("Project overview not found for project {ProjectId}", projectId);
                return NotFound(new { message = $"Project overview not found for project {projectId}" });
            }

            // Check If-None-Match header for conditional request
            var etag = $"\"{result.Version}\"";
            if (Request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
            {
                var ifNoneMatch = Request.Headers[HeaderNames.IfNoneMatch].ToString();
                if (ifNoneMatch == etag)
                {
                    _logger.LogDebug("Project overview for project {ProjectId} not modified (ETag match)", projectId);
                    return StatusCode(StatusCodes.Status304NotModified);
                }
            }

            // Add ETag and cache headers
            Response.Headers[HeaderNames.ETag] = etag;
            Response.Headers[HeaderNames.CacheControl] = "private, max-age=60";
            Response.Headers[HeaderNames.Expires] = DateTimeOffset.UtcNow.AddSeconds(60).ToString("R");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project overview for project {ProjectId}", projectId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving the project overview.", message = ex.Message });
        }
    }

    /// <summary>
    /// Get multiple project overviews (for dashboard)
    /// </summary>
    /// <param name="organizationId">Optional organization ID filter</param>
    /// <param name="status">Optional status filter (Active, Archived)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of project overviews</returns>
    /// <response code="200">Project overviews retrieved successfully</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("project-overviews")]
    [ProducesResponseType(typeof(PagedResponse<ProjectOverviewReadModelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectOverviews(
        [FromQuery] int? organizationId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug(
                "Getting project overviews: OrganizationId={OrganizationId}, Status={Status}, Page={Page}, PageSize={PageSize}",
                organizationId,
                status,
                page,
                pageSize);

            var query = new GetProjectOverviewsQuery
            {
                OrganizationId = organizationId,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, ct);

            // Add cache headers for paginated results
            Response.Headers[HeaderNames.CacheControl] = "private, max-age=30";
            Response.Headers[HeaderNames.Expires] = DateTimeOffset.UtcNow.AddSeconds(30).ToString("R");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project overviews");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving project overviews.", message = ex.Message });
        }
    }
}

