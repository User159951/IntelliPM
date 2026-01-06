using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Organizations.Commands;
using IntelliPM.Application.Organizations.Queries;
using IntelliPM.Application.Organizations.DTOs;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.API.Authorization;
using Microsoft.Extensions.Logging;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.API.Controllers.SuperAdmin;

/// <summary>
/// Controller for managing organization permission policies (SuperAdmin only).
/// Provides endpoints for viewing and updating permission policies per organization.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/superadmin/organizations")]
[ApiVersion("1.0")]
[RequireSuperAdmin]
public class SuperAdminPermissionPolicyController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SuperAdminPermissionPolicyController> _logger;

    public SuperAdminPermissionPolicyController(
        IMediator mediator,
        ILogger<SuperAdminPermissionPolicyController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get organization permission policy by organization ID (SuperAdmin only).
    /// </summary>
    /// <param name="orgId">Organization ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Organization permission policy details</returns>
    [HttpGet("{orgId}/permission-policy")]
    [ProducesResponseType(typeof(OrganizationPermissionPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationPermissionPolicy(
        int orgId,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetOrganizationPermissionPolicyQuery { OrganizationId = orgId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get organization {OrganizationId} permission policy.", orgId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization {OrganizationId} not found.", orgId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization {OrganizationId} permission policy.", orgId);
            return Problem(
                title: "Error retrieving organization permission policy",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Upsert (create or update) organization permission policy (SuperAdmin only).
    /// </summary>
    /// <param name="orgId">Organization ID</param>
    /// <param name="request">Policy update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated organization permission policy</returns>
    [HttpPut("{orgId}/permission-policy")]
    [ProducesResponseType(typeof(OrganizationPermissionPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertOrganizationPermissionPolicy(
        int orgId,
        [FromBody] UpdateOrganizationPermissionPolicyRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new UpsertOrganizationPermissionPolicyCommand
            {
                OrganizationId = orgId,
                AllowedPermissions = request.AllowedPermissions ?? new List<string>(),
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update organization {OrganizationId} permission policy.", orgId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization {OrganizationId} not found.", orgId);
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating organization {OrganizationId} permission policy.", orgId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId} permission policy.", orgId);
            return Problem(
                title: "Error updating organization permission policy",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

