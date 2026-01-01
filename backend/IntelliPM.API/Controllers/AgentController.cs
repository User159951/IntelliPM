using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Application.Agent.Queries;
using IntelliPM.Application.Agents.Commands;
using System.Security.Claims;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("ai")] // Lenient rate limit for AI endpoints
public class AgentController : BaseApiController
{
    private readonly IAgentService _agentService;
    private readonly IMediator _mediator;
    private readonly ILogger<AgentController> _logger;
    
    public AgentController(
        IAgentService agentService,
        IMediator mediator,
        ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Improves a messy task description using AI with automatic function calling
    /// </summary>
    [HttpPost("improve-task")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ImproveTask(
        [FromBody] ImproveTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("📝 User {UserId} requesting task improvement (length: {Length})", 
                userId, request.Description?.Length ?? 0);
            
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { error = "Task description cannot be empty" });
            }

            if (request.Description.Length > 5000)
            {
                return BadRequest(new { error = "Task description is too long (max 5000 characters)" });
            }
            
            var result = await _agentService.ImproveTaskDescriptionAsync(
                request.Description, 
                cancellationToken);
            
            _logger.LogInformation("✅ Task improvement completed: Status={Status}, Time={Time}ms", 
                result.Status, result.ExecutionTimeMs);
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when improving task");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when improving task");
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error improving task description");
            return Problem(
                title: "Error improving task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Analyzes project risks using AI
    /// </summary>
    [HttpGet("analyze-risks/{projectId}")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeProjectRisks(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🔍 User {UserId} requesting risk analysis for project {ProjectId}", 
                userId, projectId);
            
            var result = await _agentService.AnalyzeProjectRisksAsync(
                projectId, 
                cancellationToken);
            
            _logger.LogInformation("✅ Risk analysis completed: Status={Status}, Time={Time}ms", 
                result.Status, result.ExecutionTimeMs);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found", projectId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing project {ProjectId} risks", projectId);
            return Problem(
                title: "Error analyzing project risks",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Gets paginated audit log of all agent executions
    /// </summary>
    [HttpGet("audit-log")]
    [ProducesResponseType(typeof(GetAgentAuditLogsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditLog(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? agentId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📊 Fetching agent audit logs: Page={Page}, PageSize={PageSize}, AgentId={AgentId}, Status={Status}",
                page, pageSize, agentId, status);

            if (page < 1)
            {
                return BadRequest(new { error = "Page must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "PageSize must be between 1 and 100" });
            }
            
            var query = new GetAgentAuditLogsQuery 
            { 
                Page = page, 
                PageSize = pageSize,
                AgentId = agentId,
                UserId = userId,
                Status = status
            };
            
            var result = await _mediator.Send(query, cancellationToken);
            
            _logger.LogInformation("✅ Returned {Count} logs (page {Page}/{TotalPages})", 
                result.Logs.Count, result.Page, result.TotalPages);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching agent audit logs");
            return Problem(
                title: "Error retrieving audit logs",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Gets agent execution statistics and metrics
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AgentMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📊 Fetching agent metrics");
            
            var query = new GetAgentMetricsQuery();
            var result = await _mediator.Send(query, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching agent metrics");
            return Problem(
                title: "Error retrieving metrics",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Analyze project using AI agent
    /// </summary>
    /// <param name="projectId">Project ID to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent execution result with analysis</returns>
    /// <response code="200">Analysis completed successfully</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Error during analysis</response>
    [HttpPost("analyze-project/{projectId}")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyzeProject(int projectId, CancellationToken cancellationToken = default)
    {
        var command = new AnalyzeProjectCommand(projectId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Status == "Error")
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Detect risks in a project using AI agent
    /// </summary>
    /// <param name="projectId">Project ID to analyze for risks</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent execution result with detected risks</returns>
    /// <response code="200">Risk detection completed successfully</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Error during risk detection</response>
    [HttpPost("detect-risks/{projectId}")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DetectRisks(int projectId, CancellationToken cancellationToken = default)
    {
        var command = new DetectRisksCommand(projectId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Plan sprint using AI agent
    /// </summary>
    /// <param name="sprintId">Sprint ID to plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent execution result with sprint plan</returns>
    /// <response code="200">Sprint planning completed successfully</response>
    /// <response code="404">Sprint not found</response>
    /// <response code="500">Error during sprint planning</response>
    [HttpPost("plan-sprint/{sprintId}")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PlanSprint(int sprintId, CancellationToken cancellationToken = default)
    {
        var command = new PlanSprintCommand(sprintId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Analyze task dependencies for a project using AI agent
    /// </summary>
    /// <param name="projectId">Project ID to analyze dependencies</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent execution result with dependency analysis</returns>
    /// <response code="200">Dependency analysis completed successfully</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Error during dependency analysis</response>
    [HttpPost("analyze-dependencies/{projectId}")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeDependencies(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🔗 User {UserId} requesting dependency analysis for project {ProjectId}", 
                userId, projectId);
            
            var result = await _agentService.AnalyzeTaskDependenciesAsync(
                projectId, 
                cancellationToken);
            
            _logger.LogInformation("✅ Dependency analysis completed: Status={Status}, Time={Time}ms", 
                result.Status, result.ExecutionTimeMs);
            
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found", projectId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing dependencies for project {ProjectId}", projectId);
            return Problem(
                title: "Error analyzing dependencies",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Generate sprint retrospective using AI agent
    /// </summary>
    /// <param name="sprintId">Sprint ID to generate retrospective for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent execution result with retrospective</returns>
    /// <response code="200">Retrospective generated successfully</response>
    /// <response code="400">Sprint is not completed</response>
    /// <response code="404">Sprint not found</response>
    /// <response code="500">Error during retrospective generation</response>
    [HttpPost("generate-retrospective/{sprintId}")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateRetrospective(
        int sprintId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("📝 User {UserId} requesting retrospective generation for sprint {SprintId}", 
                userId, sprintId);
            
            var result = await _agentService.GenerateSprintRetrospectiveAsync(
                sprintId, 
                cancellationToken);
            
            _logger.LogInformation("✅ Retrospective generation completed: Status={Status}, Time={Time}ms", 
                result.Status, result.ExecutionTimeMs);
            
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Sprint {SprintId} not found", sprintId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for sprint {SprintId}: {Message}", sprintId, ex.Message);
            return Problem(
                title: "Invalid Operation",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating retrospective for sprint {SprintId}", sprintId);
            return Problem(
                title: "Error generating retrospective",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

public record ImproveTaskRequest(string Description);

