using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Application.AI.Queries;
using IntelliPM.Application.Agent.Queries;
using IntelliPM.Application.AI.Commands;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Common.Interfaces;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Controller for AI governance endpoints.
/// Provides access to AI decision logs, quota status, and usage statistics for the current organization.
/// </summary>
[ApiController]
[Route("api/v1/ai")]
[Authorize]
public class AIGovernanceController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AIGovernanceController> _logger;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the AIGovernanceController.
    /// </summary>
    public AIGovernanceController(
        IMediator mediator, 
        ILogger<AIGovernanceController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get AI decision logs for the current organization.
    /// Supports filtering by decision type, agent type, entity, date range, and approval status.
    /// </summary>
    /// <param name="decisionType">Filter by decision type (e.g., "RiskDetection", "SprintPlanning")</param>
    /// <param name="agentType">Filter by agent type (e.g., "DeliveryAgent", "ProductAgent")</param>
    /// <param name="entityType">Filter by entity type (e.g., "Project", "Sprint", "Task")</param>
    /// <param name="entityId">Filter by specific entity ID</param>
    /// <param name="startDate">Start date for filtering decisions</param>
    /// <param name="endDate">End date for filtering decisions</param>
    /// <param name="requiresApproval">Filter by approval requirement status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of AI decision logs</returns>
    [HttpGet("decisions")]
    [ProducesResponseType(typeof(PagedResponse<AIDecisionLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDecisions(
        [FromQuery] string? decisionType,
        [FromQuery] string? agentType,
        [FromQuery] string? entityType,
        [FromQuery] int? entityId,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        [FromQuery] bool? requiresApproval,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetAIDecisionLogsQuery
            {
                OrganizationId = _currentUserService.GetOrganizationId(),
                DecisionType = decisionType,
                AgentType = agentType,
                EntityType = entityType,
                EntityId = entityId,
                StartDate = startDate,
                EndDate = endDate,
                RequiresApproval = requiresApproval,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI decisions");
            return Problem(
                title: "Error retrieving AI decisions",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get specific AI decision by ID.
    /// Returns detailed information including reasoning, input/output data, and approval status.
    /// </summary>
    /// <param name="decisionId">Unique decision identifier (GUID)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Detailed AI decision log</returns>
    [HttpGet("decisions/{decisionId:guid}")]
    [ProducesResponseType(typeof(AIDecisionLogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDecisionById(Guid decisionId, CancellationToken ct)
    {
        try
        {
            var query = new GetAIDecisionByIdQuery
            {
                DecisionId = decisionId,
                OrganizationId = _currentUserService.GetOrganizationId()
            };

            var result = await _mediator.Send(query, ct);

            if (result == null)
            {
                return NotFound(new { message = $"Decision {decisionId} not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI decision {DecisionId}", decisionId);
            return Problem(
                title: "Error retrieving AI decision",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Approve an AI decision (human-in-the-loop).
    /// Allows authorized users to approve decisions that require human approval.
    /// </summary>
    /// <param name="decisionId">Unique decision identifier (GUID)</param>
    /// <param name="request">Approval request with optional notes</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("decisions/{decisionId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApproveDecision(
        Guid decisionId,
        [FromBody] ApproveDecisionRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new ApproveAIDecisionCommand
            {
                DecisionId = decisionId,
                ApprovalNotes = request.Notes
            };

            await _mediator.Send(command, ct);
            return Ok(new { message = "Decision approved successfully" });
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.UnauthorizedException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving AI decision {DecisionId}", decisionId);
            return Problem(
                title: "Error approving decision",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Reject an AI decision.
    /// Allows authorized users to reject decisions that require human approval.
    /// </summary>
    /// <param name="decisionId">Unique decision identifier (GUID)</param>
    /// <param name="request">Rejection request with reason and optional notes</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("decisions/{decisionId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RejectDecision(
        Guid decisionId,
        [FromBody] RejectDecisionRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new RejectAIDecisionCommand
            {
                DecisionId = decisionId,
                RejectionNotes = request.Notes,
                RejectionReason = request.Reason
            };

            await _mediator.Send(command, ct);
            return Ok(new { message = "Decision rejected successfully" });
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.UnauthorizedException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting AI decision {DecisionId}", decisionId);
            return Problem(
                title: "Error rejecting decision",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get current AI quota status for organization.
    /// Returns real-time quota usage, limits, and status information.
    /// </summary>
    /// <param name="organizationId">Optional organization ID. If not provided, uses current user's organization.</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current AI quota status</returns>
    [HttpGet("quota")]
    [ProducesResponseType(typeof(AIQuotaStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuotaStatus(
        [FromQuery] int? organizationId = null,
        CancellationToken ct = default)
    {
        try
        {
            // Use provided organizationId or fall back to current user's organization
            int targetOrganizationId;
            try
            {
                targetOrganizationId = organizationId ?? _currentUserService.GetOrganizationId();
            }
            catch (Exception orgEx)
            {
                _logger.LogError(orgEx, "Error getting organization ID for user {UserId}", _currentUserService.GetUserId());
                return Problem(
                    title: "Authentication Error",
                    detail: "Unable to determine organization. Please ensure you are properly authenticated.",
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }
            
            if (targetOrganizationId == 0)
            {
                _logger.LogWarning("Organization ID is required but not found for user {UserId}", _currentUserService.GetUserId());
                return BadRequest(new { error = "Organization ID is required. Please ensure you are properly authenticated." });
            }

            var query = new GetAIQuotaStatusQuery
            {
                OrganizationId = targetOrganizationId
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI quota status for organization {OrganizationId}. Exception type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                organizationId, ex.GetType().Name, ex.Message, ex.StackTrace);
            return Problem(
                title: "Error retrieving quota status",
                detail: $"An error occurred while retrieving quota status: {ex.Message}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get AI usage statistics for current organization.
    /// Provides aggregated usage data including tokens, requests, decisions, and costs.
    /// </summary>
    /// <param name="startDate">Start date for statistics (default: 30 days ago)</param>
    /// <param name="endDate">End date for statistics (default: now)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>AI usage statistics</returns>
    [HttpGet("usage/statistics")]
    [ProducesResponseType(typeof(AIUsageStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsageStatistics(
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken ct)
    {
        try
        {
            var query = new GetAIUsageStatisticsQuery
            {
                OrganizationId = _currentUserService.GetOrganizationId(),
                StartDate = startDate ?? DateTimeOffset.UtcNow.AddDays(-30),
                EndDate = endDate ?? DateTimeOffset.UtcNow
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI usage statistics");
            return Problem(
                title: "Error retrieving usage statistics",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get agent execution logs for performance monitoring and analysis.
    /// Returns detailed execution logs with performance metrics, success/failure status, and token usage.
    /// </summary>
    /// <param name="agentId">Filter by agent ID (e.g., "delivery-agent")</param>
    /// <param name="agentType">Filter by agent type (e.g., "DeliveryAgent", "ProductAgent")</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="status">Filter by status (e.g., "Success", "Error")</param>
    /// <param name="success">Filter by success status (true/false)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of agent execution logs</returns>
    [HttpGet("executions")]
    [ProducesResponseType(typeof(GetAgentAuditLogsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetExecutionLogs(
        [FromQuery] string? agentId,
        [FromQuery] string? agentType,
        [FromQuery] string? userId,
        [FromQuery] string? status,
        [FromQuery] bool? success,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var query = new Application.Agent.Queries.GetAgentAuditLogsQuery
            {
                Page = page,
                PageSize = pageSize,
                AgentId = agentId,
                AgentType = agentType,
                UserId = userId,
                Status = status,
                Success = success
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent execution logs");
            return Problem(
                title: "Error retrieving execution logs",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

/// <summary>
/// Request model for approving an AI decision.
/// </summary>
public record ApproveDecisionRequest(string? Notes);

/// <summary>
/// Request model for rejecting an AI decision.
/// </summary>
public record RejectDecisionRequest(string? Notes, string Reason);

