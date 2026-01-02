using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Tasks.Queries;
using IntelliPM.Application.Tasks.Commands;
using IntelliPM.Application.Tasks.DTOs;
using System.Security.Claims;
using IntelliPM.API.Authorization;
using IntelliPM.Application.Common.Exceptions;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class TasksController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all tasks for a project with optional filters
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="status">Optional status filter (Todo, InProgress, Blocked, Done)</param>
    /// <param name="assigneeId">Optional assignee ID filter</param>
    /// <param name="priority">Optional priority filter (Low, Medium, High, Critical)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of tasks matching the filters</returns>
    /// <response code="200">Returns the list of tasks</response>
    /// <response code="400">Bad request - Invalid filter parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("project/{projectId}")]
    [RequirePermission("tasks.view")]
    [ProducesResponseType(typeof(GetTasksByProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTasksByProject(
        int projectId,
        [FromQuery] string? status = null,
        [FromQuery] int? assigneeId = null,
        [FromQuery] string? priority = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting tasks for project {ProjectId} with filters: Status={Status}, AssigneeId={AssigneeId}, Priority={Priority}", 
                projectId, status, assigneeId, priority);
            
            var query = new GetTasksByProjectQuery(projectId, status, assigneeId, priority);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving tasks",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task details</returns>
    /// <response code="200">Returns the task details</response>
    /// <response code="404">Task not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{taskId}")]
    [RequirePermission("tasks.view")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTaskById(int taskId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting task by ID: {TaskId}", taskId);
            
            var query = new GetTaskByIdQuery(taskId);
            var result = await _mediator.Send(query, ct);
            
            if (result == null)
            {
                _logger.LogWarning("Task {TaskId} not found", taskId);
                return NotFound(new { message = $"Task with ID {taskId} not found" });
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task {TaskId}", taskId);
            return Problem(
                title: "Error retrieving task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all blocked tasks for a project
    /// </summary>
    [HttpGet("project/{projectId}/blocked")]
    [ProducesResponseType(typeof(GetBlockedTasksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBlockedTasks(int projectId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting blocked tasks for project {ProjectId}", projectId);
            
            var query = new GetBlockedTasksQuery(projectId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blocked tasks for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving blocked tasks",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all tasks assigned to a specific user
    /// </summary>
    [HttpGet("assignee/{assigneeId}")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTasksByAssignee(int assigneeId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting tasks for assignee {AssigneeId}", assigneeId);
            
            var query = new GetTasksByAssigneeQuery(assigneeId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for assignee {AssigneeId}", assigneeId);
            return Problem(
                title: "Error retrieving tasks",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/Tasks
    ///     {
    ///        "title": "Implement user authentication",
    ///        "description": "Add JWT-based authentication to the API",
    ///        "projectId": 1,
    ///        "priority": "High",
    ///        "storyPoints": 5,
    ///        "assigneeId": 2
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">Task creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created task</returns>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission to create tasks</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [RequirePermission("tasks.create")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} creating task: {Title} for project {ProjectId}", 
                userId, request.Title, request.ProjectId);
            
            var command = new CreateTaskCommand(
                request.Title,
                request.Description,
                request.ProjectId,
                request.Priority,
                request.StoryPoints,
                request.AssigneeId,
                userId
            );
            
            var result = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetTaskById), new { taskId = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when creating task");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating task");
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return Problem(
                title: "Error creating task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{taskId}")]
    [RequirePermission("tasks.edit")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTask(
        int taskId,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} updating task {TaskId}", userId, taskId);
            
            var command = new UpdateTaskCommand(
                taskId,
                request.Title,
                request.Description,
                request.Priority,
                request.StoryPoints,
                userId
            );
            
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when updating task {TaskId}", taskId);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Task {TaskId} not found when updating", taskId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating task {TaskId}", taskId);
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", taskId);
            return Problem(
                title: "Error updating task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Change task status
    /// </summary>
    [HttpPatch("{taskId}/status")]
    [RequirePermission("tasks.edit")]
    [ProducesResponseType(typeof(ChangeTaskStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeTaskStatus(
        int taskId,
        [FromBody] ChangeTaskStatusRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} changing status of task {TaskId} to {NewStatus}", 
                userId, taskId, request.NewStatus);
            
            var command = new ChangeTaskStatusCommand(taskId, request.NewStatus, userId);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when changing task {TaskId} status", taskId);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Task {TaskId} not found when changing status", taskId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid status for task {TaskId}", taskId);
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status of task {TaskId}", taskId);
            return Problem(
                title: "Error changing task status",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Assign or unassign a task to a user
    /// </summary>
    [HttpPatch("{taskId}/assign")]
    [RequirePermission("tasks.assign")]
    [ProducesResponseType(typeof(AssignTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignTask(
        int taskId,
        [FromBody] AssignTaskRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} assigning task {TaskId} to {AssigneeId}", 
                userId, taskId, request.AssigneeId);
            
            var command = new AssignTaskCommand(taskId, request.AssigneeId, userId);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when assigning task {TaskId}", taskId);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Task {TaskId} not found when assigning", taskId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid assignee for task {TaskId}", taskId);
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task {TaskId}", taskId);
            return Problem(
                title: "Error assigning task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Add a dependency to a task
    /// </summary>
    /// <param name="taskId">The task that depends on another task (source task)</param>
    /// <param name="request">Dependency creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created dependency information</returns>
    /// <response code="201">Dependency created successfully</response>
    /// <response code="400">Bad request - Validation failed or cycle detected</response>
    /// <response code="404">Task not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{taskId}/dependencies")]
    [RequirePermission("tasks.dependencies.create")]
    [ProducesResponseType(typeof(TaskDependencyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTaskDependency(
        int taskId,
        [FromBody] AddTaskDependencyRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(
                "Adding dependency to task {TaskId}: dependentTaskId={DependentTaskId}, type={DependencyType}",
                taskId, request.DependentTaskId, request.DependencyType);

            var command = new AddTaskDependencyCommand
            {
                SourceTaskId = taskId,
                DependentTaskId = request.DependentTaskId,
                DependencyType = ParseDependencyType(request.DependencyType)
            };

            var result = await _mediator.Send(command, ct);
            return CreatedAtAction(
                nameof(GetTaskDependencies),
                new { taskId = taskId },
                result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when adding dependency to task {TaskId}", taskId);
            return BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Task not found when adding dependency to task {TaskId}", taskId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding dependency to task {TaskId}", taskId);
            return Problem(
                title: "Error creating dependency",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Remove a task dependency
    /// </summary>
    /// <param name="dependencyId">The ID of the dependency to remove</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Dependency removed successfully</response>
    /// <response code="404">Dependency not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("dependencies/{dependencyId}")]
    [RequirePermission("tasks.dependencies.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveTaskDependency(
        int dependencyId,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Removing task dependency {DependencyId}", dependencyId);

            var command = new RemoveTaskDependencyCommand
            {
                DependencyId = dependencyId
            };

            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Dependency {DependencyId} not found when removing", dependencyId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing dependency {DependencyId}", dependencyId);
            return Problem(
                title: "Error removing dependency",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all dependencies for a specific task
    /// </summary>
    /// <param name="taskId">The task ID to get dependencies for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of dependencies</returns>
    /// <response code="200">Dependencies retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{taskId}/dependencies")]
    [RequirePermission("tasks.view")]
    [ProducesResponseType(typeof(List<TaskDependencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTaskDependencies(
        int taskId,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting dependencies for task {TaskId}", taskId);

            var query = new GetTaskDependenciesQuery
            {
                TaskId = taskId
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dependencies for task {TaskId}", taskId);
            return Problem(
                title: "Error retrieving dependencies",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }


    /// <summary>
    /// Parse dependency type string to enum
    /// </summary>
    private static Domain.Enums.DependencyType ParseDependencyType(string dependencyType)
    {
        return dependencyType switch
        {
            "FinishToStart" => Domain.Enums.DependencyType.FinishToStart,
            "StartToStart" => Domain.Enums.DependencyType.StartToStart,
            "FinishToFinish" => Domain.Enums.DependencyType.FinishToFinish,
            "StartToFinish" => Domain.Enums.DependencyType.StartToFinish,
            _ => throw new ArgumentException($"Invalid dependency type: {dependencyType}")
        };
    }
}

public record CreateTaskRequest(
    string Title,
    string Description,
    int ProjectId,
    string Priority,
    int? StoryPoints = null,
    int? AssigneeId = null
);

public record UpdateTaskRequest(
    string? Title = null,
    string? Description = null,
    string? Priority = null,
    int? StoryPoints = null
);

public record ChangeTaskStatusRequest(string NewStatus);

public record AssignTaskRequest(int? AssigneeId);

public record AddTaskDependencyRequest(
    int DependentTaskId,
    string DependencyType // "FinishToStart", "StartToStart", "FinishToFinish", "StartToFinish"
);
