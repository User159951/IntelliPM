using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Organizations.Commands;
using IntelliPM.Application.Organizations.Queries;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.API.Authorization;
using Microsoft.Extensions.Logging;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for managing organizations (SuperAdmin only).
/// Provides CRUD operations for organizations.
/// </summary>
[ApiController]
[Route("api/admin/organizations")]
[ApiVersion("1.0")]
[RequireSuperAdmin]
public class OrganizationsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrganizationsController> _logger;

    public OrganizationsController(
        IMediator mediator,
        ILogger<OrganizationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get a paginated list of organizations (SuperAdmin only).
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search term for filtering by name or code</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of organizations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrganizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetOrganizationsQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get organizations.");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizations.");
            return Problem(
                title: "Error retrieving organizations",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get a single organization by ID (SuperAdmin only).
    /// </summary>
    /// <param name="orgId">Organization ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Organization details</returns>
    [HttpGet("{orgId}")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationById(
        int orgId,
        CancellationToken ct = default)
    {
        try
        {
            // Validate orgId
            if (orgId <= 0)
            {
                _logger.LogWarning("Invalid organization ID: {OrganizationId}", orgId);
                return BadRequest(new { message = "Organization ID must be greater than 0" });
            }

            var query = new GetOrganizationByIdQuery { OrganizationId = orgId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error getting organization {OrganizationId}", orgId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get organization {OrganizationId}.", orgId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization {OrganizationId} not found.", orgId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization {OrganizationId}.", orgId);
            return Problem(
                title: "Error retrieving organization",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Create a new organization (SuperAdmin only).
    /// </summary>
    /// <param name="command">Organization creation details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created organization</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrganizationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateOrganization(
        [FromBody] CreateOrganizationCommand command,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _mediator.Send(command, ct);
            return CreatedAtAction(
                nameof(GetOrganizationById),
                new { orgId = result.OrganizationId },
                result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to create organization.");
            return Forbid();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating organization.");
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error creating organization.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization.");
            return Problem(
                title: "Error creating organization",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update an organization (SuperAdmin only).
    /// </summary>
    /// <param name="orgId">Organization ID</param>
    /// <param name="command">Organization update details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated organization</returns>
    [HttpPut("{orgId}")]
    [ProducesResponseType(typeof(UpdateOrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrganization(
        int orgId,
        [FromBody] UpdateOrganizationCommand command,
        CancellationToken ct = default)
    {
        try
        {
            if (orgId != command.OrganizationId)
            {
                return BadRequest(new { message = "Organization ID in route must match Organization ID in body." });
            }

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update organization {OrganizationId}.", orgId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization {OrganizationId} not found.", orgId);
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating organization {OrganizationId}.", orgId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error updating organization {OrganizationId}.", orgId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId}.", orgId);
            return Problem(
                title: "Error updating organization",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Delete an organization (SuperAdmin only).
    /// </summary>
    /// <param name="orgId">Organization ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{orgId}")]
    [ProducesResponseType(typeof(DeleteOrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrganization(
        int orgId,
        CancellationToken ct = default)
    {
        try
        {
            var command = new DeleteOrganizationCommand { OrganizationId = orgId };
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to delete organization {OrganizationId}.", orgId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization {OrganizationId} not found.", orgId);
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error deleting organization {OrganizationId}.", orgId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error deleting organization {OrganizationId}.", orgId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {OrganizationId}.", orgId);
            return Problem(
                title: "Error deleting organization",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

