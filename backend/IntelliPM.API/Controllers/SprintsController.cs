using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Sprints.Commands;
using IntelliPM.Application.Sprints.Queries;
using IntelliPM.Application.Common.Exceptions;
using System.Security.Claims;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SprintsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SprintsController> _logger;

    public SprintsController(IMediator mediator, ILogger<SprintsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all sprints for a project
    /// </summary>
    [HttpGet("project/{projectId}")]
    [RequirePermission("sprints.view")]
    [ProducesResponseType(typeof(GetSprintsByProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSprintsByProject(int projectId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting sprints for project {ProjectId}", projectId);
            
            var query = new GetSprintsByProjectQuery(projectId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sprints for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving sprints",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get sprint by ID with tasks
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("sprints.view")]
    [ProducesResponseType(typeof(SprintDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSprintById(int id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting sprint by ID: {SprintId}", id);
            
            var query = new GetSprintByIdQuery(id);
            var result = await _mediator.Send(query, ct);
            
            return Ok(result);
        }
        catch (IntelliPM.Application.Common.Exceptions.NotFoundException)
        {
            // NotFoundException is handled by global exception handler
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sprint {SprintId}", id);
            return Problem(
                title: "Error retrieving sprint",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Create a new sprint
    /// </summary>
    [HttpPost]
    [RequirePermission("sprints.create")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSprint(
        [FromBody] CreateSprintRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} creating sprint: {Name} for project {ProjectId}", 
                userId, req.Name, req.ProjectId);
            
            var cmd = new CreateSprintCommand(
                req.Name,
                req.ProjectId,
                userId,
                req.StartDate,
                req.EndDate,
                req.Capacity,
                req.Goal
            );
            var result = await _mediator.Send(cmd, ct);
            return CreatedAtAction(nameof(GetSprintById), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when creating sprint");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating sprint");
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating sprint");
            return Problem(
                title: "Invalid Operation",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sprint");
            return Problem(
                title: "Error creating sprint",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Assign tasks to a sprint
    /// </summary>
    [HttpPost("{id}/assign-tasks")]
    [ProducesResponseType(typeof(AssignTasksToSprintResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignTasksToSprint(
        int id,
        [FromBody] AssignTasksRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} assigning {TaskCount} tasks to sprint {SprintId}", 
                userId, req.TaskIds.Count, id);
            
            var cmd = new AssignTasksToSprintCommand(id, req.TaskIds, userId);
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when assigning tasks to sprint {SprintId}", id);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Sprint {SprintId} not found when assigning tasks", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when assigning tasks to sprint {SprintId}", id);
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning tasks to sprint {SprintId}", id);
            return Problem(
                title: "Error assigning tasks",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Add tasks to a sprint with automatic velocity calculation.
    /// </summary>
    /// <param name="sprintId">The ID of the sprint</param>
    /// <param name="request">The request containing task IDs and optional capacity warning flag</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing sprint details and capacity information</returns>
    /// <response code="200">Tasks added successfully</response>
    /// <response code="207">Tasks added but sprint is over capacity (warning in response)</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User doesn't have permission to manage sprints</response>
    /// <response code="404">Not Found - Sprint or tasks not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{sprintId}/add-tasks")]
    [RequirePermission("sprints.manage")]
    [Authorize]
    [ProducesResponseType(typeof(AddTaskToSprintResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AddTaskToSprintResponse), StatusCodes.Status207MultiStatus)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTasksToSprint(
        int sprintId,
        [FromBody] AddTasksToSprintRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Adding {TaskCount} task(s) to sprint {SprintId}", request.TaskIds.Count, sprintId);

            var cmd = new AddTaskToSprintCommand
            {
                SprintId = sprintId,
                TaskIds = request.TaskIds,
                IgnoreCapacityWarning = request.IgnoreCapacityWarning ?? false
            };

            var result = await _mediator.Send(cmd, ct);

            // Return 207 Multi-Status if over capacity
            if (result.IsOverCapacity)
            {
                return StatusCode(StatusCodes.Status207MultiStatus, result);
            }

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when adding tasks to sprint {SprintId}", sprintId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when adding tasks to sprint {SprintId}", sprintId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Sprint or tasks not found when adding tasks to sprint {SprintId}", sprintId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tasks to sprint {SprintId}", sprintId);
            return Problem(
                title: "Error adding tasks to sprint",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Remove tasks from a sprint and return them to the backlog.
    /// </summary>
    /// <param name="sprintId">The ID of the sprint</param>
    /// <param name="request">The request containing task IDs to remove</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing sprint details and updated capacity information</returns>
    /// <response code="200">Tasks removed successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User doesn't have permission to manage sprints</response>
    /// <response code="404">Not Found - Sprint or tasks not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{sprintId}/remove-tasks")]
    [RequirePermission("sprints.manage")]
    [Authorize]
    [ProducesResponseType(typeof(RemoveTaskFromSprintResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveTasksFromSprint(
        int sprintId,
        [FromBody] RemoveTasksFromSprintRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Removing {TaskCount} task(s) from sprint {SprintId}", request.TaskIds.Count, sprintId);

            var cmd = new RemoveTaskFromSprintCommand
            {
                SprintId = sprintId,
                TaskIds = request.TaskIds
            };

            var result = await _mediator.Send(cmd, ct);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when removing tasks from sprint {SprintId}", sprintId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when removing tasks from sprint {SprintId}", sprintId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Sprint or tasks not found when removing tasks from sprint {SprintId}", sprintId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tasks from sprint {SprintId}", sprintId);
            return Problem(
                title: "Error removing tasks from sprint",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Start a sprint (change status to Active)
    /// </summary>
    [HttpPatch("{id}/start")]
    [ProducesResponseType(typeof(StartSprintResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartSprint(int id, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} starting sprint {SprintId}", userId, id);
            
            var cmd = new StartSprintCommand(id, userId);
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when starting sprint {SprintId}", id);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Sprint {SprintId} not found when starting", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when starting sprint {SprintId}", id);
            return Problem(
                title: "Invalid Operation",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting sprint {SprintId}", id);
            return Problem(
                title: "Error starting sprint",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Complete a sprint (change status to Completed)
    /// </summary>
    [HttpPatch("{id}/complete")]
    [RequirePermission("sprints.manage")]
    [ProducesResponseType(typeof(CompleteSprintResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteSprint(
        int id,
        [FromBody] CompleteSprintRequest? request = null,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} completing sprint {SprintId}", userId, id);
            
            var cmd = new CompleteSprintCommand(
                id,
                userId,
                request?.IncompleteTasksAction
            );
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when completing sprint {SprintId}", id);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Sprint {SprintId} not found when completing", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when completing sprint {SprintId}", id);
            return Problem(
                title: "Invalid Operation",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing sprint {SprintId}", id);
            return Problem(
                title: "Error completing sprint",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

public record CreateSprintRequest(
    string Name,
    int ProjectId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Capacity,
    string? Goal = null
);

public record AssignTasksRequest(List<int> TaskIds);

public record AddTasksToSprintRequest(
    List<int> TaskIds,
    bool? IgnoreCapacityWarning = false
);

public record RemoveTasksFromSprintRequest(
    List<int> TaskIds
);

public record CompleteSprintRequest(string? IncompleteTasksAction);

