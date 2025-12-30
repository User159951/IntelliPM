using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Agents.Commands;

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
public class AgentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Run Product Agent for project analysis
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result</returns>
    [HttpPost("run-product")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
    /// <param name="projectId">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result</returns>
    [HttpPost("run-delivery")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
    /// <param name="projectId">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result</returns>
    [HttpPost("run-manager")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
    /// <param name="projectId">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result</returns>
    [HttpPost("run-qa")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
    /// <param name="projectId">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Agent execution result</returns>
    [HttpPost("run-business")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
    /// <param name="projectId">Project ID</param>
    /// <param name="req">Note type and content</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stored note result</returns>
    [HttpPost("notes")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StoreNote(int projectId, [FromBody] StoreNoteRequest req, CancellationToken ct)
    {
        var cmd = new StoreNoteCommand(projectId, req.Type, req.Content);
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }
}

public record StoreNoteRequest(string Type, string Content);

