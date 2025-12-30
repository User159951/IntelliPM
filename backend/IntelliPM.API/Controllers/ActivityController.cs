using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Activity.Queries;
using System.Security.Claims;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class ActivityController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ActivityController> _logger;

    public ActivityController(IMediator mediator, ILogger<ActivityController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get recent activities for user's projects
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(GetRecentActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentActivity(
        [FromQuery] int limit = 10,
        [FromQuery] int? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting recent activity: Limit={Limit}, ProjectId={ProjectId}, UserId={UserId}", limit, projectId, userId);

            var query = new GetRecentActivityQuery
            {
                Limit = limit,
                ProjectId = projectId,
                UserId = userId, // Filter by current user's projects
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activity");
            return Problem(
                title: "Error retrieving activity",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
