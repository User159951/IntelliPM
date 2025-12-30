using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Backlog.Commands;
using IntelliPM.Application.Backlog.Queries;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/projects/{projectId}/backlog")]
[ApiVersion("1.0")]
[Authorize]
public class BacklogController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<BacklogController> _logger;

    public BacklogController(IMediator mediator, ILogger<BacklogController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new user story in the project backlog
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="req">Story creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created story</returns>
    /// <response code="201">Story created successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("stories")]
    [RequirePermission("backlog.create")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateStory(
        int projectId,
        [FromBody] CreateBacklogItemRequest req,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var cmd = new CreateBacklogItemCommand(
            projectId, currentUserId, "Story", req.Title, req.Description, req.StoryPoints, req.DomainTag,
            null, req.FeatureId, req.AcceptanceCriteria);
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(CreateStory), result);
    }

    /// <summary>
    /// Create a new feature in the project backlog
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="req">Feature creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created feature</returns>
    /// <response code="201">Feature created successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("features")]
    [RequirePermission("backlog.create")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFeature(
        int projectId,
        [FromBody] CreateBacklogItemRequest req,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var cmd = new CreateBacklogItemCommand(
            projectId, currentUserId, "Feature", req.Title, req.Description, req.StoryPoints, req.DomainTag,
            req.EpicId, null, null);
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(CreateFeature), result);
    }

    /// <summary>
    /// Create a new epic in the project backlog
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="req">Epic creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created epic</returns>
    /// <response code="201">Epic created successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("epics")]
    [RequirePermission("backlog.create")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEpic(
        int projectId,
        [FromBody] CreateBacklogItemRequest req,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var cmd = new CreateBacklogItemCommand(
            projectId, currentUserId, "Epic", req.Title, req.Description, null, null, null, null, null);
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(CreateEpic), result);
    }

    /// <summary>
    /// Get backlog tasks (unassigned tasks) for a project sorted by priority.
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <param name="priority">Optional priority filter: "Critical", "High", "Medium", "Low"</param>
    /// <param name="status">Optional status filter: "Todo", "InProgress", "Done"</param>
    /// <param name="searchTerm">Optional search term to filter by title or description</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated response with backlog tasks sorted by priority</returns>
    /// <response code="200">Backlog tasks retrieved successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User doesn't have access to this project</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("tasks")]
    [RequirePermission("backlog.view")]
    [ProducesResponseType(typeof(PagedResponse<BacklogTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBacklog(
        int projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? priority = null,
        [FromQuery] string? status = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(
                "Getting backlog for project {ProjectId} (Page {Page}, PageSize {PageSize}, Priority: {Priority}, Status: {Status}, SearchTerm: {SearchTerm})",
                projectId, page, pageSize, priority ?? "All", status ?? "All", searchTerm ?? "None");

            var query = new GetBacklogQuery
            {
                ProjectId = projectId,
                Page = page,
                PageSize = pageSize,
                Priority = priority,
                Status = status,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when getting backlog for project {ProjectId}", projectId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when getting backlog for project {ProjectId}", projectId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting backlog for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving backlog",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

public record CreateBacklogItemRequest(
    string Title,
    string Description,
    int? StoryPoints,
    string? DomainTag,
    int? EpicId,
    int? FeatureId,
    string? AcceptanceCriteria
);

