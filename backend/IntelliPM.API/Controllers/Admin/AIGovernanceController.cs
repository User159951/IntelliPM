using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Application.AI.Commands;
using IntelliPM.Application.AI.Queries;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Application.Common.Models;
using System.Text;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Admin controller for AI governance endpoints.
/// Provides administrative access to manage AI quotas, disable/enable AI features, and view cross-organization data.
/// </summary>
[ApiController]
[Route("api/admin/ai")]
[Authorize(Roles = "Admin")]
public class AdminAIGovernanceController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminAIGovernanceController> _logger;

    /// <summary>
    /// Initializes a new instance of the AdminAIGovernanceController.
    /// </summary>
    public AdminAIGovernanceController(IMediator mediator, ILogger<AdminAIGovernanceController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Update AI quota for an organization (Admin only).
    /// Allows administrators to modify quota limits, change tiers, and configure overage settings.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="request">Quota update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated quota information</returns>
    [HttpPut("quota/{organizationId}")]
    [ProducesResponseType(typeof(UpdateAIQuotaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuota(
        int organizationId,
        [FromBody] UpdateQuotaRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new UpdateAIQuotaCommand
            {
                OrganizationId = organizationId,
                TierName = request.TierName,
                MaxTokensPerPeriod = request.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = request.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = request.MaxCostPerPeriod,
                AllowOverage = request.AllowOverage,
                OverageRate = request.OverageRate,
                EnforceQuota = request.EnforceQuota,
                ApplyImmediately = request.ApplyImmediately,
                Reason = request.Reason
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI quota for organization {OrganizationId}", organizationId);
            return Problem(
                title: "Error updating quota",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Disable AI for an organization (Kill switch).
    /// Emergency kill switch to immediately disable all AI features for an organization.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="request">Disable request with reason and options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Disable operation result</returns>
    [HttpPost("disable/{organizationId}")]
    [ProducesResponseType(typeof(DisableAIForOrgResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableAI(
        int organizationId,
        [FromBody] DisableAIRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new DisableAIForOrgCommand
            {
                OrganizationId = organizationId,
                Reason = request.Reason,
                NotifyOrganization = request.NotifyOrganization,
                Mode = request.IsPermanent ? DisableMode.Permanent : DisableMode.Temporary
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling AI for organization {OrganizationId}", organizationId);
            return Problem(
                title: "Error disabling AI",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Enable AI for an organization.
    /// Re-enables AI features for an organization that was previously disabled.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="request">Enable request with tier and reason</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Enable operation result</returns>
    [HttpPost("enable/{organizationId}")]
    [ProducesResponseType(typeof(EnableAIForOrgResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableAI(
        int organizationId,
        [FromBody] EnableAIRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new EnableAIForOrgCommand
            {
                OrganizationId = organizationId,
                TierName = request.TierName,
                Reason = request.Reason
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling AI for organization {OrganizationId}", organizationId);
            return Problem(
                title: "Error enabling AI",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all AI quotas (Admin only).
    /// Returns paginated list of all organization quotas with filtering options.
    /// </summary>
    /// <param name="tierName">Filter by tier name</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="isExceeded">Filter by exceeded status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of AI quotas</returns>
    [HttpGet("quotas")]
    [ProducesResponseType(typeof(PagedResponse<AIQuotaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllQuotas(
        [FromQuery] string? tierName,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isExceeded,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetAllAIQuotasQuery
            {
                TierName = tierName,
                IsActive = isActive,
                IsExceeded = isExceeded,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all AI quotas");
            return Problem(
                title: "Error retrieving quotas",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get AI decisions across all organizations (Admin only).
    /// Returns paginated list of decisions from all organizations with filtering options.
    /// </summary>
    /// <param name="organizationId">Filter by organization ID</param>
    /// <param name="decisionType">Filter by decision type</param>
    /// <param name="agentType">Filter by agent type</param>
    /// <param name="startDate">Start date for filtering</param>
    /// <param name="endDate">End date for filtering</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of AI decision logs</returns>
    [HttpGet("decisions/all")]
    [ProducesResponseType(typeof(PagedResponse<AIDecisionLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllDecisions(
        [FromQuery] int? organizationId,
        [FromQuery] string? decisionType,
        [FromQuery] string? agentType,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetAllAIDecisionLogsQuery
            {
                OrganizationId = organizationId,
                DecisionType = decisionType,
                AgentType = agentType,
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all AI decisions");
            return Problem(
                title: "Error retrieving decisions",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Export AI decisions to CSV (Admin only).
    /// Generates a CSV file with all AI decisions for compliance and reporting purposes.
    /// </summary>
    /// <param name="organizationId">Filter by organization ID (optional)</param>
    /// <param name="startDate">Start date for export (default: 30 days ago)</param>
    /// <param name="endDate">End date for export (default: now)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>CSV file download</returns>
    [HttpGet("decisions/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportDecisions(
        [FromQuery] int? organizationId,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken ct)
    {
        try
        {
            var query = new ExportAIDecisionsQuery
            {
                OrganizationId = organizationId,
                StartDate = startDate ?? DateTimeOffset.UtcNow.AddDays(-30),
                EndDate = endDate ?? DateTimeOffset.UtcNow
            };

            var csvContent = await _mediator.Send(query, ct);
            var bytes = Encoding.UTF8.GetBytes(csvContent);

            var fileName = $"ai_decisions_{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting AI decisions");
            return Problem(
                title: "Error exporting decisions",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get AI overview statistics aggregated across all organizations (Admin only).
    /// Returns comprehensive statistics including organization counts, decision metrics, top agents, and quota breakdown.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>AI overview statistics</returns>
    [HttpGet("overview/stats")]
    [ProducesResponseType(typeof(AIOverviewStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOverviewStats(CancellationToken ct = default)
    {
        try
        {
            var query = new GetAIOverviewStatsQuery();
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI overview stats");
            return Problem(
                title: "Error retrieving overview stats",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

/// <summary>
/// Request model for updating AI quota.
/// </summary>
public record UpdateQuotaRequest(
    string TierName,
    int? MaxTokensPerPeriod,
    int? MaxRequestsPerPeriod,
    int? MaxDecisionsPerPeriod,
    decimal? MaxCostPerPeriod,
    bool? AllowOverage,
    decimal? OverageRate,
    bool? EnforceQuota,
    bool ApplyImmediately,
    string? Reason
);

/// <summary>
/// Request model for disabling AI for an organization.
/// </summary>
public record DisableAIRequest(
    string Reason,
    bool NotifyOrganization,
    bool IsPermanent
);

/// <summary>
/// Request model for enabling AI for an organization.
/// </summary>
public record EnableAIRequest(
    string TierName,
    string Reason
);

