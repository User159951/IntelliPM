using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Projections.Commands;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for managing read model projections (Admin only).
/// </summary>
[ApiController]
[Route("api/admin/read-models")]
[ApiVersion("1.0")]
[RequireAdmin]
public class ReadModelsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReadModelsController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ReadModelsController(
        IMediator mediator,
        ILogger<ReadModelsController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Rebuild read model projections from source data.
    /// Admin-only operation for maintaining projection consistency.
    /// </summary>
    /// <param name="request">Rebuild projection request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Rebuild statistics and details</returns>
    /// <response code="200">Projections rebuilt successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost("rebuild")]
    [ProducesResponseType(typeof(RebuildProjectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RebuildProjections(
        [FromBody] RebuildProjectionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            // Double-check admin authorization
            if (!_currentUserService.IsAdmin())
            {
                _logger.LogWarning("User {UserId} attempted to rebuild projections without admin privileges", _currentUserService.GetUserId());
                return Forbid();
            }

            _logger.LogInformation(
                "Admin user {UserId} initiating projection rebuild: Type={ProjectionType}, ProjectId={ProjectId}, OrganizationId={OrganizationId}, ForceRebuild={ForceRebuild}",
                _currentUserService.GetUserId(),
                request.ProjectionType,
                request.ProjectId,
                request.OrganizationId,
                request.ForceRebuild);

            var command = new RebuildProjectionCommand
            {
                ProjectionType = request.ProjectionType ?? "All",
                ProjectId = request.ProjectId,
                OrganizationId = request.OrganizationId,
                ForceRebuild = request.ForceRebuild
            };

            var result = await _mediator.Send(command, ct);

            if (!result.Success)
            {
                _logger.LogError("Projection rebuild failed: {Error}", result.Error);
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            _logger.LogInformation(
                "Projection rebuild completed successfully: {Count} projections rebuilt in {Duration}ms",
                result.ProjectionsRebuilt,
                result.Duration.TotalMilliseconds);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding projections: {Message}", ex.Message);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while rebuilding projections.", message = ex.Message });
        }
    }
}

/// <summary>
/// Request model for rebuilding projections.
/// </summary>
public class RebuildProjectionRequest
{
    /// <summary>
    /// Type of projection to rebuild: "All", "TaskBoard", "SprintSummary", "ProjectOverview"
    /// </summary>
    public string? ProjectionType { get; set; }

    /// <summary>
    /// Optional: Rebuild projections for a specific project only
    /// </summary>
    public int? ProjectId { get; set; }

    /// <summary>
    /// Optional: Rebuild projections for all projects in a specific organization
    /// </summary>
    public int? OrganizationId { get; set; }

    /// <summary>
    /// If true, delete existing read models and rebuild from scratch.
    /// </summary>
    public bool ForceRebuild { get; set; } = false;
}

