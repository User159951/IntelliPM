using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Defects.Commands;
using IntelliPM.Application.Defects.Queries;
using System.Security.Claims;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/projects/{projectId}/defects")]
[ApiVersion("1.0")]
[Authorize]
public class DefectsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<DefectsController> _logger;

    public DefectsController(IMediator mediator, ILogger<DefectsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all defects for a project with optional filters
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="severity">Optional severity filter</param>
    /// <param name="assignedToId">Optional assignee filter</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of defects</returns>
    /// <response code="200">Defects retrieved successfully</response>
    /// <response code="500">Error retrieving defects</response>
    [HttpGet]
    [RequirePermission("defects.view")]
    [ProducesResponseType(typeof(GetProjectDefectsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefects(
        int projectId, 
        [FromQuery] string? status,
        [FromQuery] string? severity,
        [FromQuery] int? assignedToId,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetProjectDefectsQuery(projectId, status, severity, assignedToId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting defects for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving defects",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get a specific defect by ID
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="id">Defect ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Defect details</returns>
    /// <response code="200">Defect retrieved successfully</response>
    /// <response code="404">Defect not found</response>
    /// <response code="500">Error retrieving defect</response>
    [HttpGet("{id}")]
    [RequirePermission("defects.view")]
    [ProducesResponseType(typeof(DefectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDefectById(int projectId, int id, CancellationToken ct = default)
    {
        try
        {
            var query = new GetDefectByIdQuery(id);
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
            _logger.LogError(ex, "Error getting defect {DefectId}", id);
            return Problem(
                title: "Error retrieving defect",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Create a new defect
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="req">Defect creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created defect</returns>
    /// <response code="201">Defect created successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="500">Error creating defect</response>
    [HttpPost]
    [RequirePermission("defects.create")]
    [ProducesResponseType(typeof(CreateDefectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDefect(
        int projectId,
        [FromBody] CreateDefectRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} creating defect in project {ProjectId}", userId, projectId);
            
            var cmd = new CreateDefectCommand(
                projectId,
                req.UserStoryId,
                req.SprintId,
                req.Title,
                req.Description,
                req.Severity,
                userId,
                req.FoundInEnvironment,
                req.StepsToReproduce,
                req.AssignedToId
            );
            var result = await _mediator.Send(cmd, ct);
            return CreatedAtAction(nameof(GetDefectById), new { projectId, id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating defect in project {ProjectId}", projectId);
            return Problem(
                title: "Error creating defect",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update an existing defect
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="id">Defect ID</param>
    /// <param name="req">Defect update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated defect</returns>
    /// <response code="200">Defect updated successfully</response>
    /// <response code="404">Defect not found</response>
    /// <response code="500">Error updating defect</response>
    [HttpPatch("{id}")]
    [RequirePermission("defects.edit")]
    [ProducesResponseType(typeof(UpdateDefectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateDefect(
        int projectId,
        int id,
        [FromBody] UpdateDefectRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} updating defect {DefectId}", userId, id);
            
            var cmd = new UpdateDefectCommand(
                id,
                userId,
                req.Title,
                req.Description,
                req.Severity,
                req.Status,
                req.AssignedToId,
                req.FoundInEnvironment,
                req.StepsToReproduce,
                req.Resolution
            );
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Defect {DefectId} not found for update", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating defect {DefectId}", id);
            return Problem(
                title: "Error updating defect",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Delete a defect
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="id">Defect ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Defect deleted successfully</response>
    /// <response code="404">Defect not found</response>
    /// <response code="500">Error deleting defect</response>
    [HttpDelete("{id}")]
    [RequirePermission("defects.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDefect(int projectId, int id, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} deleting defect {DefectId}", userId, id);
            
            var cmd = new DeleteDefectCommand(id, userId);
            await _mediator.Send(cmd, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Defect {DefectId} not found for deletion", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting defect {DefectId}", id);
            return Problem(
                title: "Error deleting defect",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

public record CreateDefectRequest(
    int? UserStoryId,
    int? SprintId,
    string Title,
    string Description,
    string Severity,
    string? FoundInEnvironment,
    string? StepsToReproduce,
    int? AssignedToId
);

public record UpdateDefectRequest(
    string? Title,
    string? Description,
    string? Severity,
    string? Status,
    int? AssignedToId,
    string? FoundInEnvironment,
    string? StepsToReproduce,
    string? Resolution
);

