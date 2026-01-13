using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Features.Milestones.Commands;
using IntelliPM.Application.Features.Milestones.Queries;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Enums;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Controller for managing project milestones.
/// Provides endpoints for creating, updating, completing, and deleting milestones.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class MilestonesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<MilestonesController> _logger;

    public MilestonesController(IMediator mediator, ILogger<MilestonesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all milestones for a project with optional filtering.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="status">Optional status filter (Pending, InProgress, Completed, Missed, Cancelled)</param>
    /// <param name="includeCompleted">Whether to include completed milestones (default: false)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of milestones</returns>
    /// <response code="200">Milestones retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("../projects/{projectId}/milestones")]
    [RequirePermission("milestones.view")]
    [ProducesResponseType(typeof(List<MilestoneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<MilestoneDto>>> GetProjectMilestones(
        int projectId,
        [FromQuery] string? status = null,
        [FromQuery] bool includeCompleted = false,
        CancellationToken ct = default)
    {
        try
        {
            MilestoneStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<MilestoneStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }

            var query = new GetProjectMilestonesQuery(projectId, statusEnum, includeCompleted);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting milestones for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving milestones",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get the next upcoming milestone for a project.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The next upcoming milestone, or null if none found</returns>
    /// <response code="200">Next milestone retrieved successfully</response>
    /// <response code="404">No upcoming milestones found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("../projects/{projectId}/milestones/next")]
    [RequirePermission("milestones.view")]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MilestoneDto>> GetNextMilestone(
        int projectId,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetNextMilestoneQuery(projectId);
            var result = await _mediator.Send(query, ct);
            
            if (result == null)
            {
                return NotFound(new { message = "No upcoming milestones found" });
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next milestone for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving next milestone",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get milestone statistics for a project.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Milestone statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("../projects/{projectId}/milestones/statistics")]
    [RequirePermission("milestones.view")]
    [ProducesResponseType(typeof(MilestoneStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MilestoneStatisticsDto>> GetStatistics(
        int projectId,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetMilestoneStatisticsQuery(projectId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting milestone statistics for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving statistics",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get a specific milestone by ID.
    /// </summary>
    /// <param name="id">The milestone ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The milestone</returns>
    /// <response code="200">Milestone retrieved successfully</response>
    /// <response code="404">Milestone not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("{id}")]
    [RequirePermission("milestones.view")]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MilestoneDto>> GetMilestone(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetMilestoneByIdQuery(id);
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
            _logger.LogError(ex, "Error getting milestone {MilestoneId}", id);
            return Problem(
                title: "Error retrieving milestone",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all overdue milestones across all projects.
    /// </summary>
    /// <returns>List of overdue milestones</returns>
    /// <response code="200">Overdue milestones retrieved successfully</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("overdue")]
    [RequirePermission("milestones.view")]
    [ProducesResponseType(typeof(List<MilestoneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<MilestoneDto>>> GetOverdueMilestones(CancellationToken ct = default)
    {
        try
        {
            var query = new GetOverdueMilestonesQuery();
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue milestones");
            return Problem(
                title: "Error retrieving overdue milestones",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Creates a new milestone for a project.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="request">The milestone creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The created milestone</returns>
    /// <response code="201">Milestone created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="403">Insufficient permissions</response>
    /// <response code="404">Project not found</response>
    [HttpPost("../projects/{projectId}/milestones")]
    [RequirePermission("milestones.create")]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MilestoneDto>> CreateMilestone(
        int projectId,
        [FromBody] CreateMilestoneRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new CreateMilestoneCommand
            {
                ProjectId = projectId,
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                DueDate = request.DueDate,
                Progress = request.Progress ?? 0
            };

            var result = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetMilestone), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating milestone for project {ProjectId}", projectId);
            return BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found when creating milestone", projectId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating milestone for project {ProjectId}", projectId);
            return Problem(
                title: "Error creating milestone",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Updates an existing milestone.
    /// </summary>
    /// <param name="id">The milestone ID</param>
    /// <param name="request">The milestone update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The updated milestone</returns>
    /// <response code="200">Milestone updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="403">Insufficient permissions</response>
    /// <response code="404">Milestone not found</response>
    [HttpPut("{id}")]
    [RequirePermission("milestones.edit")]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MilestoneDto>> UpdateMilestone(
        int id,
        [FromBody] UpdateMilestoneRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new UpdateMilestoneCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                DueDate = request.DueDate,
                Progress = request.Progress
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating milestone {MilestoneId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Milestone {MilestoneId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating milestone {MilestoneId}", id);
            return Problem(
                title: "Error updating milestone",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Marks a milestone as completed.
    /// </summary>
    /// <param name="id">The milestone ID</param>
    /// <param name="request">Optional completion request with completion date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The completed milestone</returns>
    /// <response code="200">Milestone completed successfully</response>
    /// <response code="400">Invalid request data or milestone cannot be completed</response>
    /// <response code="403">Insufficient permissions</response>
    /// <response code="404">Milestone not found</response>
    [HttpPost("{id}/complete")]
    [RequirePermission("milestones.complete")]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MilestoneDto>> CompleteMilestone(
        int id,
        [FromBody] CompleteMilestoneRequest? request = null,
        CancellationToken ct = default)
    {
        try
        {
            var command = new CompleteMilestoneCommand
            {
                Id = id,
                CompletedAt = request?.CompletedAt
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error completing milestone {MilestoneId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Milestone {MilestoneId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing milestone {MilestoneId}", id);
            return Problem(
                title: "Error completing milestone",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Deletes a milestone.
    /// </summary>
    /// <param name="id">The milestone ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Milestone deleted successfully</response>
    /// <response code="403">Insufficient permissions</response>
    /// <response code="404">Milestone not found</response>
    [HttpDelete("{id}")]
    [RequirePermission("milestones.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMilestone(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var command = new DeleteMilestoneCommand(id);
            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Milestone {MilestoneId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting milestone {MilestoneId}", id);
            return Problem(
                title: "Error deleting milestone",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // Request DTOs

    /// <summary>
    /// Request DTO for creating a milestone.
    /// </summary>
    public class CreateMilestoneRequest
    {
        /// <summary>
        /// Name of the milestone.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the milestone.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Type of milestone: "Release", "Sprint", "Deadline", "Custom".
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Due date for the milestone.
        /// </summary>
        public DateTimeOffset DueDate { get; set; }

        /// <summary>
        /// Progress percentage (0-100). Default: 0.
        /// </summary>
        public int? Progress { get; set; }
    }

    /// <summary>
    /// Request DTO for updating a milestone.
    /// </summary>
    public class UpdateMilestoneRequest
    {
        /// <summary>
        /// Updated name of the milestone.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Updated description of the milestone.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Updated due date for the milestone.
        /// </summary>
        public DateTimeOffset DueDate { get; set; }

        /// <summary>
        /// Updated progress percentage (0-100).
        /// </summary>
        public int Progress { get; set; }
    }

    /// <summary>
    /// Request DTO for completing a milestone.
    /// </summary>
    public class CompleteMilestoneRequest
    {
        /// <summary>
        /// Optional completion date. If not provided, uses current UTC time.
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }
    }
}

