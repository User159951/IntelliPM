using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.AI.Commands;
using IntelliPM.Application.AI.Queries;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.API.Authorization;
using Microsoft.Extensions.Logging;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.API.Controllers.SuperAdmin;

/// <summary>
/// Controller for managing organization AI quotas (SuperAdmin only).
/// Provides endpoints for viewing and updating AI quota settings per organization.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/superadmin/organizations")]
[ApiVersion("1.0")]
[RequireSuperAdmin]
public class SuperAdminAIQuotaController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SuperAdminAIQuotaController> _logger;

    public SuperAdminAIQuotaController(
        IMediator mediator,
        ILogger<SuperAdminAIQuotaController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get organization AI quota by organization ID (SuperAdmin only).
    /// </summary>
    /// <param name="orgId">Organization ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Organization AI quota details</returns>
    [HttpGet("{orgId}/ai-quota")]
    [ProducesResponseType(typeof(OrganizationAIQuotaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationAIQuota(
        int orgId,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetOrganizationAIQuotaQuery { OrganizationId = orgId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get organization {OrganizationId} AI quota.", orgId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization {OrganizationId} not found.", orgId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization {OrganizationId} AI quota.", orgId);
            return Problem(
                title: "Error retrieving organization AI quota",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Upsert (create or update) organization AI quota (SuperAdmin only).
    /// </summary>
    /// <param name="orgId">Organization ID</param>
    /// <param name="request">Quota update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated organization AI quota</returns>
    [HttpPut("{orgId}/ai-quota")]
    [ProducesResponseType(typeof(OrganizationAIQuotaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertOrganizationAIQuota(
        int orgId,
        [FromBody] UpdateOrganizationAIQuotaRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new UpsertOrganizationAIQuotaCommand
            {
                OrganizationId = orgId,
                MonthlyTokenLimit = request.MonthlyTokenLimit,
                MonthlyRequestLimit = request.MonthlyRequestLimit,
                ResetDayOfMonth = request.ResetDayOfMonth,
                IsAIEnabled = request.IsAIEnabled
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update organization {OrganizationId} AI quota.", orgId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization {OrganizationId} not found.", orgId);
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating organization {OrganizationId} AI quota.", orgId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId} AI quota.", orgId);
            return Problem(
                title: "Error updating organization AI quota",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get a paginated list of all organization AI quotas (SuperAdmin only).
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search term for filtering by organization name or code</param>
    /// <param name="isAIEnabled">Filter by AI enabled status</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of organization AI quotas</returns>
    [HttpGet("ai-quotas")]
    [ProducesResponseType(typeof(PagedResponse<OrganizationAIQuotaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrganizationAIQuotas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isAIEnabled = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetOrganizationAIQuotasQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                IsAIEnabled = isAIEnabled
            };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to list organization AI quotas.");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing organization AI quotas.");
            return Problem(
                title: "Error retrieving organization AI quotas",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

