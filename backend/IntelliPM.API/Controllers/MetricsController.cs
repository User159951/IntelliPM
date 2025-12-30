using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Queries.Metrics;
using IntelliPM.Application.Sprints.Queries;
using IntelliPM.Application.Common.Exceptions;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(IMediator mediator, ILogger<MetricsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get metrics summary for all projects or a specific project
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MetricsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMetrics([FromQuery] int? projectId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting metrics summary for projectId: {ProjectId}", projectId ?? 0);
            
            var query = new GetMetricsSummaryQuery { ProjectId = projectId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics summary");
            return Problem(
                title: "Error retrieving metrics",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get sprint velocity chart data (last 6 completed sprints)
    /// </summary>
    [HttpGet("sprint-velocity-chart")]
    [ProducesResponseType(typeof(SprintVelocityChartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSprintVelocityChart([FromQuery] int? projectId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting sprint velocity chart for projectId: {ProjectId}", projectId ?? 0);
            
            var query = new GetSprintVelocityChartQuery { ProjectId = projectId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sprint velocity chart");
            return Problem(
                title: "Error retrieving sprint velocity chart",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get task distribution by status
    /// </summary>
    [HttpGet("task-distribution")]
    [ProducesResponseType(typeof(TaskDistributionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTaskDistribution([FromQuery] int? projectId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting task distribution for projectId: {ProjectId}", projectId ?? 0);
            
            var query = new GetTaskDistributionQuery { ProjectId = projectId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task distribution");
            return Problem(
                title: "Error retrieving task distribution",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get sprint burndown chart data
    /// </summary>
    [HttpGet("sprint-burndown")]
    [ProducesResponseType(typeof(SprintBurndownResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSprintBurndown([FromQuery] int sprintId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting sprint burndown for sprintId: {SprintId}", sprintId);
            
            var query = new GetSprintBurndownQuery { SprintId = sprintId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Sprint not found: {SprintId}", sprintId);
            return Problem(
                title: "Sprint not found",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sprint burndown");
            return Problem(
                title: "Error retrieving sprint burndown",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get defects by severity
    /// </summary>
    [HttpGet("defects-by-severity")]
    [ProducesResponseType(typeof(DefectsBySeverityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDefectsBySeverity([FromQuery] int? projectId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting defects by severity for projectId: {ProjectId}", projectId ?? 0);
            
            var query = new GetDefectsBySeverityQuery { ProjectId = projectId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting defects by severity");
            return Problem(
                title: "Error retrieving defects by severity",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get team velocity trend over time
    /// </summary>
    [HttpGet("team-velocity")]
    [ProducesResponseType(typeof(TeamVelocityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamVelocity([FromQuery] int? projectId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting team velocity for projectId: {ProjectId}", projectId ?? 0);
            
            var query = new GetTeamVelocityQuery { ProjectId = projectId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team velocity");
            return Problem(
                title: "Error retrieving team velocity",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get sprint velocity based on completed story points.
    /// </summary>
    /// <param name="projectId">The ID of the project (required)</param>
    /// <param name="sprintId">Optional sprint ID. If provided, returns velocity for that sprint only. If null, returns velocity for last N sprints.</param>
    /// <param name="lastNSprints">Number of sprints to retrieve for trend analysis (default: 5, max: 20). Only used when sprintId is not provided.</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing sprint velocity data with completed story points, planned story points, and completion rates</returns>
    /// <response code="200">Sprint velocity retrieved successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User doesn't have access to this project</response>
    /// <response code="404">Not Found - Project or sprint not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("velocity")]
    [ProducesResponseType(typeof(SprintVelocityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSprintVelocity(
        [FromQuery] int projectId,
        [FromQuery] int? sprintId = null,
        [FromQuery] int? lastNSprints = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(
                "Getting sprint velocity for project {ProjectId}, sprintId: {SprintId}, lastNSprints: {LastNSprints}",
                projectId,
                sprintId?.ToString() ?? "All",
                lastNSprints?.ToString() ?? "5");

            var query = new GetSprintVelocityQuery
            {
                ProjectId = projectId,
                SprintId = sprintId,
                LastNSprints = lastNSprints
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when getting sprint velocity for project {ProjectId}", projectId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when getting sprint velocity for project {ProjectId}", projectId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project or sprint not found when getting sprint velocity for project {ProjectId}", projectId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sprint velocity for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving sprint velocity",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

