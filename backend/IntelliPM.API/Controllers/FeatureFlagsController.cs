using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.FeatureFlags.Queries;
using IntelliPM.Application.Common.Interfaces;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Controller for reading feature flags (accessible to all authenticated users).
/// For managing feature flags (create/update), use the admin endpoint.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/feature-flags")]
[ApiVersion("1.0")]
[Authorize]
public class FeatureFlagsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FeatureFlagsController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public FeatureFlagsController(
        IMediator mediator,
        ILogger<FeatureFlagsController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Get all feature flags for the current user's organization.
    /// </summary>
    /// <param name="organizationId">Optional organization ID to filter feature flags. If not provided, uses current user's organization.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of feature flags</returns>
    /// <response code="200">Returns the list of feature flags</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<FeatureFlagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllFeatureFlags(
        [FromQuery] int? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use current user's organization if not specified
            if (!organizationId.HasValue)
            {
                organizationId = _currentUserService.GetOrganizationId();
            }

            var query = new GetAllFeatureFlagsQuery(organizationId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flags");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while retrieving feature flags." });
        }
    }

    /// <summary>
    /// Get a feature flag by name for the current user's organization.
    /// </summary>
    /// <param name="name">The name of the feature flag</param>
    /// <param name="organizationId">Optional organization ID to filter feature flags. If not provided, uses current user's organization.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The feature flag if found</returns>
    /// <response code="200">Returns the feature flag</response>
    /// <response code="404">Feature flag not found</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFeatureFlagByName(
        [FromRoute] string name,
        [FromQuery] int? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use current user's organization if not specified
            if (!organizationId.HasValue)
            {
                organizationId = _currentUserService.GetOrganizationId();
            }

            var query = new GetFeatureFlagByNameQuery(name, organizationId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new { error = $"Feature flag '{name}' not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flag by name: {Name}", name);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while retrieving the feature flag." });
        }
    }
}

