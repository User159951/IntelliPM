using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Admin.Commands;
using IntelliPM.Application.Identity.Commands;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for managing users (Admin only).
/// </summary>
[ApiController]
[Route("api/admin/users")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IMediator mediator,
        ILogger<UsersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invite a user to the organization.
    /// </summary>
    /// <param name="command">Invitation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Invitation details including invitation ID and link</returns>
    /// <response code="200">Invitation sent successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="409">Conflict - User already exists or pending invitation exists</response>
    [HttpPost("invite")]
    [RequirePermission("users.create")]
    [ProducesResponseType(typeof(InviteOrganizationUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InviteUser(
        [FromBody] InviteOrganizationUserCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error inviting user: {Email}", command.Email);
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to invite user: {Email}", command.Email);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when inviting user: {Email}", command.Email);
            return Problem(
                title: "Conflict",
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting user: {Email}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                command.Email, 
                ex.GetType().Name, 
                ex.Message, 
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while sending the invitation.", details = ex.Message });
        }
    }

    /// <summary>
    /// Activate a user account.
    /// </summary>
    /// <param name="userId">The ID of the user to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activated user information</returns>
    /// <response code="200">User activated successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="404">Not Found - User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{userId}/activate")]
    [RequirePermission("users.manage")]
    [ProducesResponseType(typeof(ActivateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ActivateUser(
        [FromRoute] int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ActivateUserCommand(userId);
            var result = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("User {UserId} activated successfully", userId);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error activating user: {UserId}", userId);
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to activate user: {UserId}", userId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", userId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                userId,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while activating the user.", details = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a user account.
    /// </summary>
    /// <param name="userId">The ID of the user to deactivate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deactivated user information</returns>
    /// <response code="200">User deactivated successfully</response>
    /// <response code="400">Bad request - Validation failed or cannot deactivate own account</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="404">Not Found - User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{userId}/deactivate")]
    [RequirePermission("users.manage")]
    [ProducesResponseType(typeof(DeactivateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeactivateUser(
        [FromRoute] int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeactivateUserCommand(userId);
            var result = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("User {UserId} deactivated successfully", userId);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error deactivating user: {UserId}", userId);
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to deactivate user: {UserId}", userId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", userId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                userId,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while deactivating the user.", details = ex.Message });
        }
    }
}

