using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Reports.Queries;
using IntelliPM.Application.Reports.DTOs;
using IntelliPM.Application.Common.Interfaces;
using System.Text;
using Microsoft.Extensions.Logging;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for role-based reporting endpoints (Admin only).
/// Provides visibility into "who did what" by role, AI decision tracking, and workflow transitions.
/// </summary>
[ApiController]
[Route("api/admin/reports")]
[ApiVersion("1.0")]
[RequireAdmin]
public class ReportsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<ReportsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get activity report grouped by user role.
    /// Shows actions grouped by user role with counts and last performed dates.
    /// </summary>
    /// <param name="startDate">Start date for filtering (optional)</param>
    /// <param name="endDate">End date for filtering (optional)</param>
    /// <param name="roleFilter">Filter by specific role (optional)</param>
    /// <param name="actionTypeFilter">Filter by specific action type (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of role activity reports</returns>
    [HttpGet("activity-by-role")]
    [ProducesResponseType(typeof(List<RoleActivityReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityByRole(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] string? actionTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Enforce organization access control
            int? organizationId = null;
            if (!_currentUserService.IsSuperAdmin())
            {
                organizationId = _currentUserService.GetOrganizationId();
                if (organizationId == 0)
                {
                    return Forbid("User not authenticated or organization not found");
                }
            }

            var query = new GetActivityByRoleQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                RoleFilter = roleFilter,
                ActionTypeFilter = actionTypeFilter,
                OrganizationId = organizationId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity by role report");
            return Problem(
                title: "Error retrieving report",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get AI decision report grouped by approver role.
    /// Shows AI decisions by who approved/rejected them, with response times and confidence scores.
    /// </summary>
    /// <param name="startDate">Start date for filtering (optional)</param>
    /// <param name="endDate">End date for filtering (optional)</param>
    /// <param name="roleFilter">Filter by specific role (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of AI decision role reports</returns>
    [HttpGet("ai-decisions-by-role")]
    [ProducesResponseType(typeof(List<AIDecisionRoleReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAIDecisionsByRole(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? roleFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Enforce organization access control
            int? organizationId = null;
            if (!_currentUserService.IsSuperAdmin())
            {
                organizationId = _currentUserService.GetOrganizationId();
                if (organizationId == 0)
                {
                    return Forbid("User not authenticated or organization not found");
                }
            }

            var query = new GetAIDecisionsByApproverRoleQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                RoleFilter = roleFilter,
                OrganizationId = organizationId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI decisions by role report");
            return Problem(
                title: "Error retrieving report",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get workflow transition report grouped by user role.
    /// Shows status changes grouped by role with transition counts.
    /// </summary>
    /// <param name="startDate">Start date for filtering (optional)</param>
    /// <param name="endDate">End date for filtering (optional)</param>
    /// <param name="roleFilter">Filter by specific role (optional)</param>
    /// <param name="entityTypeFilter">Filter by entity type (task, sprint, project, etc.) (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workflow role reports</returns>
    [HttpGet("workflow-transitions-by-role")]
    [ProducesResponseType(typeof(List<WorkflowRoleReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkflowTransitionsByRole(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] string? entityTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Enforce organization access control
            int? organizationId = null;
            if (!_currentUserService.IsSuperAdmin())
            {
                organizationId = _currentUserService.GetOrganizationId();
                if (organizationId == 0)
                {
                    return Forbid("User not authenticated or organization not found");
                }
            }

            var query = new GetWorkflowTransitionsByRoleQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                RoleFilter = roleFilter,
                EntityTypeFilter = entityTypeFilter,
                OrganizationId = organizationId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow transitions by role report");
            return Problem(
                title: "Error retrieving report",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Export activity by role report to CSV.
    /// </summary>
    [HttpGet("activity-by-role/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportActivityByRole(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] string? actionTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            int? organizationId = null;
            if (!_currentUserService.IsSuperAdmin())
            {
                organizationId = _currentUserService.GetOrganizationId();
                if (organizationId == 0)
                {
                    return Forbid("User not authenticated or organization not found");
                }
            }

            var query = new GetActivityByRoleQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                RoleFilter = roleFilter,
                ActionTypeFilter = actionTypeFilter,
                OrganizationId = organizationId
            };

            var data = await _mediator.Send(query, cancellationToken);
            var csvContent = GenerateActivityByRoleCsv(data);
            var bytes = Encoding.UTF8.GetBytes(csvContent);

            var fileName = $"activity_by_role_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting activity by role report");
            return Problem(
                title: "Error exporting report",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Export AI decisions by role report to CSV.
    /// </summary>
    [HttpGet("ai-decisions-by-role/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAIDecisionsByRole(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? roleFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            int? organizationId = null;
            if (!_currentUserService.IsSuperAdmin())
            {
                organizationId = _currentUserService.GetOrganizationId();
                if (organizationId == 0)
                {
                    return Forbid("User not authenticated or organization not found");
                }
            }

            var query = new GetAIDecisionsByApproverRoleQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                RoleFilter = roleFilter,
                OrganizationId = organizationId
            };

            var data = await _mediator.Send(query, cancellationToken);
            var csvContent = GenerateAIDecisionsByRoleCsv(data);
            var bytes = Encoding.UTF8.GetBytes(csvContent);

            var fileName = $"ai_decisions_by_role_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting AI decisions by role report");
            return Problem(
                title: "Error exporting report",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Export workflow transitions by role report to CSV.
    /// </summary>
    [HttpGet("workflow-transitions-by-role/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportWorkflowTransitionsByRole(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] string? entityTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            int? organizationId = null;
            if (!_currentUserService.IsSuperAdmin())
            {
                organizationId = _currentUserService.GetOrganizationId();
                if (organizationId == 0)
                {
                    return Forbid("User not authenticated or organization not found");
                }
            }

            var query = new GetWorkflowTransitionsByRoleQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                RoleFilter = roleFilter,
                EntityTypeFilter = entityTypeFilter,
                OrganizationId = organizationId
            };

            var data = await _mediator.Send(query, cancellationToken);
            var csvContent = GenerateWorkflowTransitionsByRoleCsv(data);
            var bytes = Encoding.UTF8.GetBytes(csvContent);

            var fileName = $"workflow_transitions_by_role_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting workflow transitions by role report");
            return Problem(
                title: "Error exporting report",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static string GenerateActivityByRoleCsv(List<RoleActivityReportDto> data)
    {
        var csv = new StringBuilder();
        
        // Add BOM for Excel compatibility
        csv.Append('\uFEFF');
        
        // Header
        csv.AppendLine("Role,ActionType,Count,LastPerformed,UniqueUsers");
        
        // Data rows
        foreach (var item in data)
        {
            csv.AppendLine($"{EscapeCsvField(item.Role)},{EscapeCsvField(item.ActionType)},{item.Count},{item.LastPerformed:yyyy-MM-dd HH:mm:ss},{item.UniqueUsers}");
        }
        
        return csv.ToString();
    }

    private static string GenerateAIDecisionsByRoleCsv(List<AIDecisionRoleReportDto> data)
    {
        var csv = new StringBuilder();
        
        // Add BOM for Excel compatibility
        csv.Append('\uFEFF');
        
        // Header
        csv.AppendLine("Role,DecisionsApproved,DecisionsRejected,DecisionsPending,AverageResponseTimeHours,UniqueApprovers,AverageConfidenceScore");
        
        // Data rows
        foreach (var item in data)
        {
            csv.AppendLine($"{EscapeCsvField(item.Role)},{item.DecisionsApproved},{item.DecisionsRejected},{item.DecisionsPending},{item.AverageResponseTimeHours:F2},{item.UniqueApprovers},{item.AverageConfidenceScore:F4}");
        }
        
        return csv.ToString();
    }

    private static string GenerateWorkflowTransitionsByRoleCsv(List<WorkflowRoleReportDto> data)
    {
        var csv = new StringBuilder();
        
        // Add BOM for Excel compatibility
        csv.Append('\uFEFF');
        
        // Header
        csv.AppendLine("Role,FromStatus,ToStatus,EntityType,TransitionCount,LastTransition,UniqueUsers");
        
        // Data rows
        foreach (var item in data)
        {
            csv.AppendLine($"{EscapeCsvField(item.Role)},{EscapeCsvField(item.FromStatus)},{EscapeCsvField(item.ToStatus)},{EscapeCsvField(item.EntityType)},{item.TransitionCount},{item.LastTransition:yyyy-MM-dd HH:mm:ss},{item.UniqueUsers}");
        }
        
        return csv.ToString();
    }

    private static string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;
        
        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        
        return field;
    }
}

