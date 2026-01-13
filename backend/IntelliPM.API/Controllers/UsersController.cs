using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Users.Queries;
using IntelliPM.Application.Users.Commands;
using IntelliPM.Application.Identity.Queries;
using IntelliPM.Application.Identity.DTOs;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Application.Activity.Queries;
using IntelliPM.Domain.Enums;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(
        IMediator mediator,
        ILogger<UsersController> logger,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all users in the organization (paginated).
    /// Supports filtering by role and active status, sorting, and search.
    /// Available to all authenticated users (read-only for non-admins).
    /// </summary>
    /// <param name="page">Page number (1-based). Default: 1</param>
    /// <param name="pageSize">Number of items per page. Default: 20, Max: 100</param>
    /// <param name="role">Filter by global role (Admin or User). Optional.</param>
    /// <param name="isActive">Filter by active status. Optional.</param>
    /// <param name="sortField">Field to sort by. Valid values: Username, Email, CreatedAt, LastLoginAt, Role, IsActive. Default: CreatedAt</param>
    /// <param name="sortDescending">Sort in descending order. Default: false (ascending)</param>
    /// <param name="searchTerm">Search term to filter users by username, email, firstName, or lastName (case-insensitive). Optional.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="403">Forbidden - User is not an admin</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [RequirePermission("users.view")]
    [ProducesResponseType(typeof(PagedResponse<IntelliPM.Application.Identity.DTOs.UserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortField = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // All authenticated users can access this endpoint
            // (Admin-only restrictions removed to allow regular users to view team members)

            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest(new { error = "Page must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "PageSize must be between 1 and 100" });
            }

            // Parse role enum
            GlobalRole? parsedRole = null;
            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<GlobalRole>(role, true, out var roleValue))
            {
                parsedRole = roleValue;
            }

            var query = new GetUsersQuery
            {
                Page = page,
                PageSize = pageSize,
                Role = parsedRole,
                IsActive = isActive,
                SortField = sortField,
                SortDescending = sortDescending,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error retrieving users");
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return Problem(
                title: "Error retrieving users",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update a user (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Admin only
            if (!_currentUserService.IsAdmin())
            {
                return Forbid();
            }

            Domain.Enums.GlobalRole? globalRole = null;
            if (!string.IsNullOrWhiteSpace(request.GlobalRole))
            {
                if (Enum.TryParse<Domain.Enums.GlobalRole>(request.GlobalRole, out var parsedRole))
                {
                    globalRole = parsedRole;
                }
                else
                {
                    return BadRequest(new { error = $"Invalid GlobalRole: {request.GlobalRole}" });
                }
            }

            var cmd = new UpdateUserCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Email,
                globalRole
            );

            var result = await _mediator.Send(cmd, cancellationToken);
            return Ok(result);
        }
        catch (IntelliPM.Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (IntelliPM.Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (IntelliPM.Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return Problem(
                title: "Error updating user",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Delete a user permanently (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("users.delete")]
    [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Admin only
            if (!_currentUserService.IsAdmin())
            {
                return Forbid();
            }

            var cmd = new DeleteUserCommand(id);
            var result = await _mediator.Send(cmd, cancellationToken);
            return Ok(result);
        }
        catch (IntelliPM.Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (IntelliPM.Application.Common.Exceptions.ValidationException ex)
        {
            _logger.LogWarning("Validation error deleting user {UserId}: {Message}. Errors: {@Errors}", id, ex.Message, ex.Errors);
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (IntelliPM.Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return Problem(
                title: "Error deleting user",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Bulk update user status (activate/deactivate multiple users, admin only)
    /// </summary>
    [HttpPost("bulk-status")]
    [ProducesResponseType(typeof(BulkUpdateUsersStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BulkUpdateUsersStatus(
        [FromBody] BulkUpdateUsersStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Admin only
            if (!_currentUserService.IsAdmin())
            {
                return Forbid();
            }

            var cmd = new BulkUpdateUsersStatusCommand(request.UserIds, request.IsActive);
            var result = await _mediator.Send(cmd, cancellationToken);
            return Ok(result);
        }
        catch (IntelliPM.Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (IntelliPM.Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating users status");
            return Problem(
                title: "Error bulk updating users status",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get user's projects (admin only)
    /// </summary>
    [HttpGet("{id}/projects")]
    [ProducesResponseType(typeof(PagedResponse<ProjectListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProjects(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_currentUserService.IsAdmin())
            {
                return Forbid();
            }

            var query = new GetUserProjectsQuery(id, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user projects for user {UserId}", id);
            return Problem(
                title: "Error retrieving user projects",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get user's activity history (admin only)
    /// </summary>
    [HttpGet("{id}/activity")]
    [RequirePermission("users.view")] // Admin only in practice, but using users.view
    [ProducesResponseType(typeof(GetRecentActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserActivity(
        int id,
        [FromQuery] int? limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_currentUserService.IsAdmin())
            {
                return Forbid();
            }

            var query = new GetRecentActivityQuery
            {
                UserId = id,
                Limit = limit
            };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity for user {UserId}", id);
            return Problem(
                title: "Error retrieving user activity",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

// Request DTOs
public record BulkUpdateUsersStatusRequest(
    List<int> UserIds,
    bool IsActive
);

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? GlobalRole
);
