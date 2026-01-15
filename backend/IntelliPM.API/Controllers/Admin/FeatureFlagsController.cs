using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.FeatureFlags.Queries;
using IntelliPM.Application.FeatureFlags.Commands;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for managing feature flags (Admin only).
/// </summary>
[ApiController]
[Route("api/admin/feature-flags")]
[ApiVersion("1.0")]
[RequireAdmin]
public class FeatureFlagsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FeatureFlagsController> _logger;

    public FeatureFlagsController(
        IMediator mediator,
        ILogger<FeatureFlagsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all feature flags, optionally filtered by organization ID.
    /// </summary>
    /// <param name="organizationId">Optional organization ID to filter feature flags. If not provided, returns all flags.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of feature flags</returns>
    /// <response code="200">Feature flags retrieved successfully</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<FeatureFlagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAllFeatureFlagsQuery(organizationId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flags. OrganizationId: {OrganizationId}, Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                organizationId,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving feature flags.", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new feature flag.
    /// </summary>
    /// <param name="command">Feature flag creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created feature flag</returns>
    /// <response code="201">Feature flag created successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="409">Conflict - Feature flag with same name and organization already exists</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFeatureFlagCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating feature flag: {FeatureFlagName}", command.Name);
            
            // Check if it's a duplicate conflict (contains "already exists")
            if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Problem(
                    title: "Conflict",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
            
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature flag: {FeatureFlagName}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                command.Name,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while creating the feature flag.", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing feature flag.
    /// </summary>
    /// <param name="id">Feature flag ID (Guid)</param>
    /// <param name="request">Feature flag update details (IsEnabled and/or Description)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated feature flag</returns>
    /// <response code="200">Feature flag updated successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="404">Not Found - Feature flag not found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateFeatureFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateFeatureFlagCommand(
                id,
                request.IsEnabled,
                request.Description);

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating feature flag: {FeatureFlagId}", id);
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Feature flag not found: {FeatureFlagId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature flag: {FeatureFlagId}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                id,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while updating the feature flag.", details = ex.Message });
        }
    }
}

/// <summary>
/// Request model for updating a feature flag.
/// </summary>
public record UpdateFeatureFlagRequest(
    bool? IsEnabled,
    string? Description
);
