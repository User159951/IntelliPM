using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Agents.Commands;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

/// <summary>
/// AI Agents Controller - Run AI agents for project analysis and insights
/// </summary>
/// <remarks>
/// This controller provides endpoints to trigger various AI agents (Product, Delivery, Manager, QA, Business)
/// for project-specific analysis and recommendations.
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/projects/{projectId}/agents")]
[ApiVersion("1.0")]
[Authorize]
public class AgentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AgentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Run Product Agent for project analysis
    /// </summary>
    /// <remarks>
    /// Analyzes the project from a product perspective, providing insights on features, user stories, and product strategy.
    /// The agent uses AI to analyze project data and generate recommendations.
    /// </remarks>
    /// <param name="projectId">Project ID to analyze</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result with product analysis</returns>
    /// <response code="200">Agent execution completed successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission or AI is disabled</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("run-product")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RunProductAgent(int projectId, CancellationToken ct)
    {
        var cmd = new RunProductAgentCommand(projectId);
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }

    /// <summary>
    /// Run Delivery Agent for project analysis
    /// </summary>
    /// <remarks>
    /// Analyzes the project from a delivery perspective, focusing on sprint planning, velocity, and delivery timelines.
    /// </remarks>
    /// <param name="projectId">Project ID to analyze</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result with delivery analysis</returns>
    /// <response code="200">Agent execution completed successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission or AI is disabled</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("run-delivery")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RunDeliveryAgent(int projectId, CancellationToken ct)
    {
        var cmd = new RunDeliveryAgentCommand(projectId);
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }

    /// <summary>
    /// Run Manager Agent for project analysis
    /// </summary>
    /// <remarks>
    /// Analyzes the project from a management perspective, providing insights on resource allocation, risks, and key decisions.
    /// </remarks>
    /// <param name="projectId">Project ID to analyze</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result with management analysis</returns>
    /// <response code="200">Agent execution completed successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission or AI is disabled</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("run-manager")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RunManagerAgent(int projectId, CancellationToken ct)
    {
        var cmd = new RunManagerAgentCommand(projectId);
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }

    /// <summary>
    /// Run QA Agent for project analysis
    /// </summary>
    /// <remarks>
    /// Analyzes the project from a quality assurance perspective, identifying defects, test coverage, and quality metrics.
    /// </remarks>
    /// <param name="projectId">Project ID to analyze</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result with QA analysis</returns>
    /// <response code="200">Agent execution completed successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission or AI is disabled</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("run-qa")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RunQAAgent(int projectId, CancellationToken ct)
    {
        var cmd = new RunQAAgentCommand(projectId);
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }

    /// <summary>
    /// Run Business Agent for project analysis
    /// </summary>
    /// <remarks>
    /// Analyzes the project from a business perspective, providing insights on ROI, business value, and strategic alignment.
    /// </remarks>
    /// <param name="projectId">Project ID to analyze</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result with business analysis</returns>
    /// <response code="200">Agent execution completed successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission or AI is disabled</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("run-business")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RunBusinessAgent(int projectId, CancellationToken ct)
    {
        var cmd = new RunBusinessAgentCommand(projectId);
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }

    /// <summary>
    /// Store a note for an AI agent
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/projects/{projectId}/agents/notes
    ///     {
    ///        "type": "Decision",
    ///        "content": "We decided to use React for the frontend"
    ///     }
    /// 
    /// </remarks>
    /// <param name="projectId">Project ID</param>
    /// <param name="req">Note type and content</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stored note result</returns>
    /// <response code="200">Note stored successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("notes")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StoreNote(int projectId, [FromBody] StoreNoteRequest req, CancellationToken ct)
    {
        var cmd = new StoreNoteCommand(projectId, req.Type, req.Content);
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }
}

public record StoreNoteRequest(string Type, string Content);

