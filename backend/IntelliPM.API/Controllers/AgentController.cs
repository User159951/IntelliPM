using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Application.Agent.Queries;
using IntelliPM.Application.Agents.Commands;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Services;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Sprints.Queries;
using IntelliPM.Domain.Constants;
using IntelliPM.Infrastructure.AI.Helpers;
using System.Security.Claims;
using System.Text.Json;

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
    private readonly IAIAvailabilityService _aiAvailabilityService;
    private readonly IAIDecisionLogger _decisionLogger;
    
    public AgentController(
        IAgentService agentService,
        IMediator mediator,
        ILogger<AgentController> logger,
        IAIAvailabilityService aiAvailabilityService,
        IAIDecisionLogger decisionLogger)
    {
        _agentService = agentService;
        _mediator = mediator;
        _logger = logger;
        _aiAvailabilityService = aiAvailabilityService;
        _decisionLogger = decisionLogger;
    }

    /// <summary>
    /// Improves a messy task description using AI with automatic function calling
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/Agent/improve-task
    ///     {
    ///        "description": "fix bug in login"
    ///     }
    /// 
    /// The AI will improve the description to be more detailed and professional.
    /// </remarks>
    /// <param name="request">Task description to improve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Improved task description</returns>
    /// <response code="200">Task description improved successfully</response>
    /// <response code="400">Bad request - Description is empty or too long (max 5000 characters)</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">AI is disabled for the organization</response>
    /// <response code="429">AI quota exceeded</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("improve-task")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ImproveTask(
        [FromBody] ImproveTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = GetOrganizationId();
            
            // Check quota before execution
            if (organizationId > 0)
            {
                await _aiAvailabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
            }
            
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
            
            // Log AI decision for audit trail
            if (userId > 0 && organizationId > 0 && result.Status == "Success")
            {
                var (promptTokens, completionTokens, totalTokens) = TokenUsageHelper.EstimateTokenUsage(
                    request.Description,
                    result.Content
                );
                
                var metadata = new Dictionary<string, object>
                {
                    { "ExecutionTimeMs", result.ExecutionTimeMs },
                    { "ExecutionCostUsd", result.ExecutionCostUsd },
                    { "ToolsCalled", result.ToolsCalled }
                };
                
                await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.ManagerAgent,
                    decisionType: "TaskImprovement",
                    reasoning: result.Content,
                    confidenceScore: 0.8m,
                    metadata: metadata,
                    userId: userId,
                    organizationId: organizationId,
                    entityType: "Task",
                    entityId: 0,
                    question: $"Improve task description: {request.Description}",
                    decision: result.Content,
                    inputData: JsonSerializer.Serialize(new { Description = request.Description }),
                    outputData: result.Content,
                    tokensUsed: totalTokens,
                    promptTokens: promptTokens,
                    completionTokens: completionTokens,
                    executionTimeMs: result.ExecutionTimeMs,
                    isSuccess: result.Status == "Success",
                    errorMessage: result.ErrorMessage,
                    cancellationToken: cancellationToken);
            }
            
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
        catch (AIDisabledException ex)
        {
            _logger.LogWarning(ex, "AI disabled for organization {OrganizationId}", ex.OrganizationId);
            return Problem(
                title: "AI Disabled",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (AIQuotaExceededException ex)
        {
            _logger.LogWarning(ex, "AI quota exceeded for organization {OrganizationId}. Type: {QuotaType}, Current: {CurrentUsage}, Limit: {MaxLimit}", 
                ex.OrganizationId, ex.QuotaType, ex.CurrentUsage, ex.MaxLimit);
            return Problem(
                title: "Quota Exceeded",
                detail: ex.Message,
                statusCode: StatusCodes.Status429TooManyRequests
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
    /// <param name="projectId">Project ID to analyze for risks</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent execution result with detected risks</returns>
    /// <response code="200">Risk analysis completed successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">AI is disabled for the organization</response>
    /// <response code="404">Project not found</response>
    /// <response code="429">AI quota exceeded</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("analyze-risks/{projectId}")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeProjectRisks(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = GetOrganizationId();
            
            // Check quota before execution
            if (organizationId > 0)
            {
                await _aiAvailabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
            }
            
            _logger.LogInformation("🔍 User {UserId} requesting risk analysis for project {ProjectId}", 
                userId, projectId);
            
            var result = await _agentService.AnalyzeProjectRisksAsync(
                projectId, 
                cancellationToken);
            
            _logger.LogInformation("✅ Risk analysis completed: Status={Status}, Time={Time}ms", 
                result.Status, result.ExecutionTimeMs);
            
            return Ok(result);
        }
        catch (AIDisabledException ex)
        {
            _logger.LogWarning(ex, "AI disabled for organization {OrganizationId}", ex.OrganizationId);
            return Problem(
                title: "AI Disabled",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (AIQuotaExceededException ex)
        {
            _logger.LogWarning(ex, "AI quota exceeded for organization {OrganizationId}. Type: {QuotaType}, Current: {CurrentUsage}, Limit: {MaxLimit}", 
                ex.OrganizationId, ex.QuotaType, ex.CurrentUsage, ex.MaxLimit);
            return Problem(
                title: "Quota Exceeded",
                detail: ex.Message,
                statusCode: StatusCodes.Status429TooManyRequests
            );
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
    /// <param name="page">Page number (default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 50, minimum: 1, maximum: 100)</param>
    /// <param name="agentId">Optional filter by agent ID</param>
    /// <param name="agentType">Optional filter by agent type (e.g., DeliveryAgent, ProductAgent)</param>
    /// <param name="userId">Optional filter by user ID</param>
    /// <param name="status">Optional filter by status (Pending, Success, Error)</param>
    /// <param name="success">Optional filter by success status (true/false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of agent execution logs</returns>
    /// <response code="200">Returns the paginated audit log</response>
    /// <response code="400">Bad request - Invalid pagination parameters</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("audit-log")]
    [ProducesResponseType(typeof(GetAgentAuditLogsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditLog(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? agentId = null,
        [FromQuery] string? agentType = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? success = null,
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
                AgentType = agentType,
                UserId = userId,
                Status = status,
                Success = success
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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent execution metrics (total executions, success rate, average execution time, etc.)</returns>
    /// <response code="200">Returns the agent metrics</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AgentMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = GetOrganizationId();
            
            var command = new AnalyzeProjectCommand(projectId);
            var result = await _mediator.Send(command, cancellationToken);

            // Log AI decision for audit trail
            if (userId > 0 && organizationId > 0)
            {
                var (promptTokens, completionTokens, totalTokens) = TokenUsageHelper.EstimateTokenUsage(
                    $"Analyze project {projectId}",
                    result.Content
                );
                
                var metadata = new Dictionary<string, object>
                {
                    { "ExecutionTimeMs", result.ExecutionTimeMs },
                    { "ExecutionCostUsd", result.ExecutionCostUsd },
                    { "ToolsCalled", result.ToolsCalled },
                    { "ProjectId", projectId }
                };
                
                await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.ManagerAgent,
                    decisionType: "ProjectAnalysis",
                    reasoning: result.Content,
                    confidenceScore: 0.75m,
                    metadata: metadata,
                    userId: userId,
                    organizationId: organizationId,
                    projectId: projectId,
                    entityType: "Project",
                    entityId: projectId,
                    question: $"Analyze project {projectId}",
                    decision: result.Content,
                    inputData: JsonSerializer.Serialize(new { ProjectId = projectId }),
                    outputData: result.Content,
                    tokensUsed: totalTokens,
                    promptTokens: promptTokens,
                    completionTokens: completionTokens,
                    executionTimeMs: result.ExecutionTimeMs,
                    isSuccess: result.Status == "Success",
                    errorMessage: result.ErrorMessage,
                    cancellationToken: cancellationToken);
            }

            if (result.Status == "Error")
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Problem(
                title: "Unauthorized",
                detail: "User ID not found in claims",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
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
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = GetOrganizationId();
            
            var command = new DetectRisksCommand(projectId);
            var result = await _mediator.Send(command, cancellationToken);
            
            // Log AI decision for audit trail
            if (userId > 0 && organizationId > 0)
            {
                var (promptTokens, completionTokens, totalTokens) = TokenUsageHelper.EstimateTokenUsage(
                    $"Detect risks for project {projectId}",
                    result.Content
                );
                
                var metadata = new Dictionary<string, object>
                {
                    { "ExecutionTimeMs", result.ExecutionTimeMs },
                    { "ExecutionCostUsd", result.ExecutionCostUsd },
                    { "ToolsCalled", result.ToolsCalled },
                    { "ProjectId", projectId }
                };
                
                await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.DeliveryAgent,
                    decisionType: AIDecisionConstants.DecisionTypes.RiskDetection,
                    reasoning: result.Content,
                    confidenceScore: 0.8m,
                    metadata: metadata,
                    userId: userId,
                    organizationId: organizationId,
                    projectId: projectId,
                    entityType: "Project",
                    entityId: projectId,
                    question: $"Detect risks for project {projectId}",
                    decision: result.Content,
                    inputData: JsonSerializer.Serialize(new { ProjectId = projectId }),
                    outputData: result.Content,
                    tokensUsed: totalTokens,
                    promptTokens: promptTokens,
                    completionTokens: completionTokens,
                    executionTimeMs: result.ExecutionTimeMs,
                    isSuccess: result.Status == "Success",
                    errorMessage: result.ErrorMessage,
                    cancellationToken: cancellationToken);
            }
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Problem(
                title: "Unauthorized",
                detail: "User ID not found in claims",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
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
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = GetOrganizationId();
            
            // Get sprint to retrieve projectId
            var sprintQuery = new GetSprintByIdQuery(sprintId);
            var sprint = await _mediator.Send(sprintQuery, cancellationToken);
            
            var command = new PlanSprintCommand(sprintId);
            var result = await _mediator.Send(command, cancellationToken);
            
            // Log AI decision for audit trail
            if (userId > 0 && organizationId > 0 && sprint != null)
            {
                var (promptTokens, completionTokens, totalTokens) = TokenUsageHelper.EstimateTokenUsage(
                    $"Plan sprint {sprintId}",
                    result.Content
                );
                
                var metadata = new Dictionary<string, object>
                {
                    { "ExecutionTimeMs", result.ExecutionTimeMs },
                    { "ExecutionCostUsd", result.ExecutionCostUsd },
                    { "ToolsCalled", result.ToolsCalled },
                    { "SprintId", sprintId },
                    { "ProjectId", sprint.ProjectId }
                };
                
                await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.DeliveryAgent,
                    decisionType: AIDecisionConstants.DecisionTypes.SprintPlanning,
                    reasoning: result.Content,
                    confidenceScore: 0.75m,
                    metadata: metadata,
                    userId: userId,
                    organizationId: organizationId,
                    projectId: sprint.ProjectId,
                    entityType: "Sprint",
                    entityId: sprintId,
                    entityName: sprint.Name,
                    question: $"Plan sprint {sprintId}",
                    decision: result.Content,
                    inputData: JsonSerializer.Serialize(new { SprintId = sprintId, ProjectId = sprint.ProjectId }),
                    outputData: result.Content,
                    tokensUsed: totalTokens,
                    promptTokens: promptTokens,
                    completionTokens: completionTokens,
                    executionTimeMs: result.ExecutionTimeMs,
                    isSuccess: result.Status == "Success",
                    errorMessage: result.ErrorMessage,
                    cancellationToken: cancellationToken);
            }
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Problem(
                title: "Unauthorized",
                detail: "User ID not found in claims",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
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
            var organizationId = GetOrganizationId();
            
            _logger.LogInformation("🔗 User {UserId} requesting dependency analysis for project {ProjectId}", 
                userId, projectId);
            
            var result = await _agentService.AnalyzeTaskDependenciesAsync(
                projectId, 
                cancellationToken);
            
            _logger.LogInformation("✅ Dependency analysis completed: Status={Status}, Time={Time}ms", 
                result.Status, result.ExecutionTimeMs);
            
            // Log AI decision for audit trail
            if (userId > 0 && organizationId > 0 && result.Status == "Success")
            {
                var (promptTokens, completionTokens, totalTokens) = TokenUsageHelper.EstimateTokenUsage(
                    $"Analyze dependencies for project {projectId}",
                    result.Content
                );
                
                var metadata = new Dictionary<string, object>
                {
                    { "ExecutionTimeMs", result.ExecutionTimeMs },
                    { "ExecutionCostUsd", result.ExecutionCostUsd },
                    { "ToolsCalled", result.ToolsCalled },
                    { "ProjectId", projectId }
                };
                
                await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.DeliveryAgent,
                    decisionType: "DependencyAnalysis",
                    reasoning: result.Content,
                    confidenceScore: 0.75m,
                    metadata: metadata,
                    userId: userId,
                    organizationId: organizationId,
                    projectId: projectId,
                    entityType: "Project",
                    entityId: projectId,
                    question: $"Analyze task dependencies for project {projectId}",
                    decision: result.Content,
                    inputData: JsonSerializer.Serialize(new { ProjectId = projectId }),
                    outputData: result.Content,
                    tokensUsed: totalTokens,
                    promptTokens: promptTokens,
                    completionTokens: completionTokens,
                    executionTimeMs: result.ExecutionTimeMs,
                    isSuccess: result.Status == "Success",
                    errorMessage: result.ErrorMessage,
                    cancellationToken: cancellationToken);
            }
            
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
            var organizationId = GetOrganizationId();
            
            _logger.LogInformation("📝 User {UserId} requesting retrospective generation for sprint {SprintId}", 
                userId, sprintId);
            
            // Get sprint to retrieve projectId
            var sprintQuery = new GetSprintByIdQuery(sprintId);
            var sprint = await _mediator.Send(sprintQuery, cancellationToken);
            
            var result = await _agentService.GenerateSprintRetrospectiveAsync(
                sprintId, 
                cancellationToken);
            
            _logger.LogInformation("✅ Retrospective generation completed: Status={Status}, Time={Time}ms", 
                result.Status, result.ExecutionTimeMs);
            
            // Log AI decision for audit trail
            if (userId > 0 && organizationId > 0 && sprint != null && result.Status == "Success")
            {
                var (promptTokens, completionTokens, totalTokens) = TokenUsageHelper.EstimateTokenUsage(
                    $"Generate retrospective for sprint {sprintId}",
                    result.Content
                );
                
                var metadata = new Dictionary<string, object>
                {
                    { "ExecutionTimeMs", result.ExecutionTimeMs },
                    { "ExecutionCostUsd", result.ExecutionCostUsd },
                    { "ToolsCalled", result.ToolsCalled },
                    { "SprintId", sprintId },
                    { "ProjectId", sprint.ProjectId }
                };
                
                await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.DeliveryAgent,
                    decisionType: "SprintRetrospective",
                    reasoning: result.Content,
                    confidenceScore: 0.8m,
                    metadata: metadata,
                    userId: userId,
                    organizationId: organizationId,
                    projectId: sprint.ProjectId,
                    entityType: "Sprint",
                    entityId: sprintId,
                    entityName: sprint.Name,
                    question: $"Generate retrospective for sprint {sprintId}",
                    decision: result.Content,
                    inputData: JsonSerializer.Serialize(new { SprintId = sprintId, ProjectId = sprint.ProjectId }),
                    outputData: result.Content,
                    tokensUsed: totalTokens,
                    promptTokens: promptTokens,
                    completionTokens: completionTokens,
                    executionTimeMs: result.ExecutionTimeMs,
                    isSuccess: result.Status == "Success",
                    errorMessage: result.ErrorMessage,
                    cancellationToken: cancellationToken);
            }
            
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

