using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Insights.Queries;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/projects/{projectId}/insights")]
[ApiVersion("1.0")]
[Authorize]
public class InsightsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InsightsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get AI insights for a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="agentType">Optional agent type filter</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of AI insights</returns>
    /// <response code="200">Insights retrieved successfully</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Error retrieving insights</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInsights(
        int projectId,
        [FromQuery] string? status,
        [FromQuery] string? agentType,
        CancellationToken ct)
    {
        var query = new GetProjectInsightsQuery(projectId, status, agentType);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}

