using Asp.Versioning;
using IntelliPM.Application.Comments.Commands;
using IntelliPM.Application.Comments.Queries;
using IntelliPM.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Controller for managing comments on entities (Tasks, Projects, Sprints, etc.).
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class CommentsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<CommentsController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all comments for a specific entity
    /// </summary>
    /// <param name="entityType">Type of entity (Task, Project, Sprint, Defect, BacklogItem)</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of comments for the entity</returns>
    /// <response code="200">Comments retrieved successfully</response>
    /// <response code="400">Bad request - Invalid entity type or ID</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [RequirePermission("tasks.comment")] // Comments are typically on tasks, using tasks.comment permission
    [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetComments(
        [FromQuery] string entityType,
        [FromQuery] int entityId,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return BadRequest(new { error = "Entity type is required" });
            }

            if (entityId <= 0)
            {
                return BadRequest(new { error = "Entity ID must be greater than 0" });
            }

            var organizationId = _currentUserService.GetOrganizationId();
            if (organizationId == 0)
            {
                return Unauthorized(new { error = "Organization ID not found" });
            }

            var query = new GetCommentsQuery
            {
                EntityType = entityType,
                EntityId = entityId,
                OrganizationId = organizationId
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for {EntityType} {EntityId}", entityType, entityId);
            return Problem(
                title: "Error retrieving comments",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Add a new comment to an entity
    /// </summary>
    /// <param name="request">Comment creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created comment information</returns>
    /// <response code="200">Comment created successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="404">Entity not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [RequirePermission("tasks.comment")]
    [ProducesResponseType(typeof(AddCommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddComment(
        [FromBody] AddCommentRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = _currentUserService.GetOrganizationId();

            if (organizationId == 0)
            {
                return Unauthorized(new { error = "Organization ID not found" });
            }

            var command = new AddCommentCommand
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to {EntityType} {EntityId}", request.EntityType, request.EntityId);
            
            if (ex is Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { error = ex.Message });
            }

            if (ex is Application.Common.Exceptions.ValidationException)
            {
                return BadRequest(new { error = ex.Message });
            }

            return Problem(
                title: "Error creating comment",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update an existing comment
    /// </summary>
    /// <param name="id">Comment ID</param>
    /// <param name="request">Comment update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated comment</returns>
    /// <response code="200">Comment updated successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - Not the comment author</response>
    /// <response code="404">Comment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [RequirePermission("tasks.comment")] // Users can edit their own comments (checked in handler)
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateComment(
        int id,
        [FromBody] UpdateCommentRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = _currentUserService.GetOrganizationId();

            if (organizationId == 0)
            {
                return Unauthorized(new { error = "Organization ID not found" });
            }

            var command = new UpdateCommentCommand
            {
                CommentId = id,
                Content = request.Content,
                UserId = userId,
                OrganizationId = organizationId
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", id);

            if (ex is Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { error = ex.Message });
            }

            if (ex is Application.Common.Exceptions.UnauthorizedException)
            {
                return Unauthorized(new { error = ex.Message });
            }

            if (ex is Application.Common.Exceptions.ValidationException)
            {
                return BadRequest(new { error = ex.Message });
            }

            return Problem(
                title: "Error updating comment",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Delete a comment (soft delete)
    /// </summary>
    /// <param name="id">Comment ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Comment deleted successfully</response>
    /// <response code="401">Unauthorized - Not the comment author</response>
    /// <response code="404">Comment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [RequirePermission("tasks.comment")] // Users can delete their own comments (checked in handler)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteComment(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = _currentUserService.GetOrganizationId();

            if (organizationId == 0)
            {
                return Unauthorized(new { error = "Organization ID not found" });
            }

            var command = new DeleteCommentCommand
            {
                CommentId = id,
                UserId = userId,
                OrganizationId = organizationId
            };

            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", id);

            if (ex is Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { error = ex.Message });
            }

            if (ex is Application.Common.Exceptions.UnauthorizedException)
            {
                return Unauthorized(new { error = ex.Message });
            }

            return Problem(
                title: "Error deleting comment",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

/// <summary>
/// Request DTO for adding a comment.
/// </summary>
public class AddCommentRequest
{
    /// <summary>
    /// Type of entity (Task, Project, Sprint, Defect, BacklogItem).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Comment content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional parent comment ID for threaded comments.
    /// </summary>
    public int? ParentCommentId { get; set; }
}

/// <summary>
/// Request DTO for updating a comment.
/// </summary>
public class UpdateCommentRequest
{
    /// <summary>
    /// New comment content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

