using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Admin.DeadLetterQueue.Queries;
using IntelliPM.Application.Admin.DeadLetterQueue.Commands;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for managing dead letter queue messages (Admin only).
/// </summary>
[ApiController]
[Route("api/admin/dead-letter-queue")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class DeadLetterQueueController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeadLetterQueueController> _logger;

    public DeadLetterQueueController(
        IMediator mediator,
        ILogger<DeadLetterQueueController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all dead letter queue messages with pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="eventType">Optional filter by event type</param>
    /// <param name="startDate">Optional filter by start date</param>
    /// <param name="endDate">Optional filter by end date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of dead letter queue messages</returns>
    /// <response code="200">Dead letter queue messages retrieved successfully</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DeadLetterMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? eventType = null,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDeadLetterMessagesQuery(page, pageSize, eventType, startDate, endDate);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dead letter queue messages. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving dead letter queue messages.", details = ex.Message });
        }
    }

    /// <summary>
    /// Retry a dead letter queue message by moving it back to the outbox for reprocessing.
    /// </summary>
    /// <param name="id">Dead letter message ID (Guid)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Message moved back to outbox successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="404">Not Found - Dead letter message not found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost("{id}/retry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Retry(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RetryDeadLetterMessageCommand(id);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Dead letter message not found: {MessageId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying dead letter message: {MessageId}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                id,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrying the dead letter message.", details = ex.Message });
        }
    }

    /// <summary>
    /// Permanently delete a dead letter queue message.
    /// </summary>
    /// <param name="id">Dead letter message ID (Guid)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Message deleted successfully</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="404">Not Found - Dead letter message not found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteDeadLetterMessageCommand(id);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Dead letter message not found: {MessageId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dead letter message: {MessageId}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                id,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while deleting the dead letter message.", details = ex.Message });
        }
    }
}

