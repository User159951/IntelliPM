using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Notifications.Queries;
using IntelliPM.Application.Notifications.Commands;
using System.Security.Claims;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get notifications for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetNotificationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting notifications for user {UserId}, unreadOnly: {UnreadOnly}, limit: {Limit}", 
                userId, unreadOnly, limit);

            if (limit <= 0 || limit > 50)
            {
                limit = 10;
            }

            var organizationId = GetOrganizationId();
            var query = new GetNotificationsQuery
            {
                UserId = userId,
                OrganizationId = organizationId,
                UnreadOnly = unreadOnly,
                Limit = limit,
                Offset = 0 // Can be extended to support offset parameter
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when getting notifications");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return Problem(
                title: "Error retrieving notifications",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkNotificationRead(int id, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} marking notification {NotificationId} as read", userId, id);

            var command = new MarkNotificationReadCommand
            {
                NotificationId = id,
                UserId = userId
            };

            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when marking notification {NotificationId} as read", id);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Notification {NotificationId} not found or access denied", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return Problem(
                title: "Error updating notification",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    [HttpPatch("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAllNotificationsRead(CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} marking all notifications as read", userId);

            var command = new MarkAllNotificationsReadCommand
            {
                UserId = userId
            };

            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when marking all notifications as read");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return Problem(
                title: "Error updating notifications",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get count of unread notifications for current user
    /// </summary>
    /// <returns>Unread notification count</returns>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(GetUnreadNotificationCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetUnreadNotificationCountQuery();
            var result = await _mediator.Send(query, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count");
            return Problem(
                title: "Error retrieving unread notification count",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
