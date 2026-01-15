using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.AI.Commands;
using IntelliPM.Application.AI.Queries;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.API.Authorization;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Admin controller for managing AI quota per organization member.
/// Allows administrators to view and set per-user quota overrides.
/// Admin can only access their own organization; SuperAdmin can access all organizations.
/// </summary>
[ApiController]
[Route("api/admin/ai-quota")]
[ApiVersion("1.0")]
[RequireAdmin]
public class AdminAIQuotaController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminAIQuotaController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public AdminAIQuotaController(
        IMediator mediator, 
        ILogger<AdminAIQuotaController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get paginated list of organization members with their AI quota information.
    /// For SuperAdmin: can filter by organizationId. For Admin: uses their own organization.
    /// </summary>
    /// <param name="organizationId">Optional organization ID (SuperAdmin only). If not provided, uses current user's organization.</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search by email or name (optional)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of members with quota information</returns>
    [HttpGet("members")]
    [ProducesResponseType(typeof(PagedResponse<AdminAiQuotaMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMembers(
        [FromQuery] int? organizationId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            // CRITICAL: Enforce organization access control
            // Admin can only access their own organization
            // SuperAdmin can access any organization or all (if organizationId is null)
            if (!_currentUserService.IsSuperAdmin())
            {
                // Admin must access only their own organization
                var adminOrgId = _currentUserService.GetOrganizationId();
                if (adminOrgId == 0)
                {
                    return Forbid("User not authenticated or organization not found");
                }
                
                // Override any provided organizationId with Admin's organization
                organizationId = adminOrgId;
            }
            // SuperAdmin: organizationId can be null (all orgs) or specific org

            var query = new GetAdminAiQuotaMembersQuery
            {
                OrganizationId = organizationId,
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get AI quota members");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI quota members. Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}. Exception type: {ExceptionType}, Message: {Message}", 
                page, pageSize, searchTerm, ex.GetType().Name, ex.Message);
            return Problem(
                title: "Error retrieving members",
                detail: $"An error occurred while retrieving members: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update or create a user AI quota override.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Quota override request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated quota override information</returns>
    [HttpPut("members/{userId}")]
    [ProducesResponseType(typeof(UpdateUserAIQuotaOverrideResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMemberQuota(
        [FromRoute] int userId,
        [FromBody] UpdateMemberQuotaRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new UpdateUserAIQuotaOverrideCommand
            {
                UserId = userId,
                MaxTokensPerPeriod = request.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = request.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = request.MaxCostPerPeriod,
                Reason = request.Reason
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating quota for user {UserId}", userId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update quota for user {UserId}", userId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quota for user {UserId}. Exception type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                userId, ex.GetType().Name, ex.Message, ex.StackTrace);
            return Problem(
                title: "Error updating quota",
                detail: $"An error occurred while updating quota: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Reset (delete) a user AI quota override, reverting to organization default.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Reset operation result</returns>
    [HttpPost("members/{userId}/reset")]
    [ProducesResponseType(typeof(ResetUserAIQuotaOverrideResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetMemberQuota(
        [FromRoute] int userId,
        CancellationToken ct)
    {
        try
        {
            var command = new ResetUserAIQuotaOverrideCommand
            {
                UserId = userId
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to reset quota for user {UserId}", userId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting quota for user {UserId}", userId);
            return Problem(
                title: "Error resetting quota",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get paginated list of organization members with their effective AI quotas (new model).
    /// Uses OrganizationAIQuota and UserAIQuota entities.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search by email or name (optional)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of members with effective quota information</returns>
    [HttpGet("ai-quotas/members")]
    [ProducesResponseType(typeof(PagedResponse<MemberAIQuotaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMemberAIQuotas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetMemberAIQuotasQuery
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
            _logger.LogWarning(ex, "Unauthorized attempt to get member AI quotas");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member AI quotas");
            return Problem(
                title: "Error retrieving member AI quotas",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update or create a user AI quota override (new model).
    /// Uses OrganizationAIQuota and UserAIQuota entities.
    /// Validates that override values don't exceed organization limits.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Quota override request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated member quota information with effective quota</returns>
    [HttpPut("ai-quotas/members/{userId}")]
    [ProducesResponseType(typeof(MemberAIQuotaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMemberAIQuota(
        [FromRoute] int userId,
        [FromBody] UpdateMemberAIQuotaRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new UpdateMemberAIQuotaCommand
            {
                UserId = userId,
                MonthlyTokenLimitOverride = request.MonthlyTokenLimitOverride,
                MonthlyRequestLimitOverride = request.MonthlyRequestLimitOverride,
                IsAIEnabledOverride = request.IsAIEnabledOverride
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating member AI quota for user {UserId}", userId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update member AI quota for user {UserId}", userId);
            return Forbid();
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error updating member AI quota for user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member AI quota for user {UserId}", userId);
            return Problem(
                title: "Error updating member AI quota",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get AI quota usage history for admin dashboard.
    /// Returns daily usage data aggregated from AIDecisionLog with pagination support.
    /// </summary>
    /// <param name="organizationId">Organization ID (optional - if not provided, returns data for all organizations)</param>
    /// <param name="startDate">Start date for history (default: 30 days ago)</param>
    /// <param name="endDate">End date for history (default: now)</param>
    /// <param name="page">Page number (default: 1, minimum: 1)</param>
    /// <param name="pageSize">Page size (default: 20, minimum: 1, maximum: 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated AI quota usage history with daily breakdown</returns>
    [HttpGet("usage-history")]
    [ProducesResponseType(typeof(PagedResponse<DailyUsageHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsageHistory(
        [FromQuery] int? organizationId = null,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetAIQuotaUsageHistoryQuery
            {
                OrganizationId = organizationId,
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get AI quota usage history");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI quota usage history. OrganizationId: {OrganizationId}, StartDate: {StartDate}, EndDate: {EndDate}, Page: {Page}, PageSize: {PageSize}. Exception type: {ExceptionType}, Message: {Message}", 
                organizationId, startDate, endDate, page, pageSize, ex.GetType().Name, ex.Message);
            return Problem(
                title: "Error retrieving usage history",
                detail: $"An error occurred while retrieving usage history: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get AI quota breakdown by agent type and decision type.
    /// Provides detailed breakdown for admin dashboard.
    /// </summary>
    /// <param name="organizationId">Organization ID (optional - if not provided, returns data for all organizations)</param>
    /// <param name="period">Period for breakdown: "day", "week", "month" (default: "month")</param>
    /// <param name="startDate">Start date for breakdown (optional - defaults based on period)</param>
    /// <param name="endDate">End date for breakdown (optional - defaults to now)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>AI quota breakdown with agent and decision type details</returns>
    [HttpGet("breakdown")]
    [ProducesResponseType(typeof(AIQuotaBreakdownDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBreakdown(
        [FromQuery] int? organizationId = null,
        [FromQuery] string period = "month",
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        CancellationToken ct = default)
    {
        try
        {
            // CRITICAL: Enforce organization access control
            // Admin can only access their own organization
            // SuperAdmin can access any organization or all (if organizationId is null)
            if (!_currentUserService.IsSuperAdmin())
            {
                // Admin must access only their own organization
                var adminOrgId = _currentUserService.GetOrganizationId();
                if (adminOrgId == 0)
                {
                    return Forbid("User not authenticated or organization not found");
                }
                
                // Override any provided organizationId with Admin's organization
                organizationId = adminOrgId;
            }
            // SuperAdmin: organizationId can be null (all orgs) or specific org

            var query = new GetAIQuotaBreakdownQuery
            {
                OrganizationId = organizationId,
                Period = period,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get AI quota breakdown");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI quota breakdown. OrganizationId: {OrganizationId}, Period: {Period}, StartDate: {StartDate}, EndDate: {EndDate}. Exception type: {ExceptionType}, Message: {Message}", 
                organizationId, period, startDate, endDate, ex.GetType().Name, ex.Message);
            return Problem(
                title: "Error retrieving breakdown",
                detail: $"An error occurred while retrieving breakdown: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

/// <summary>
/// Request model for updating member quota.
/// </summary>
public record UpdateMemberQuotaRequest(
    int? MaxTokensPerPeriod,
    int? MaxRequestsPerPeriod,
    int? MaxDecisionsPerPeriod,
    decimal? MaxCostPerPeriod,
    string? Reason
);

