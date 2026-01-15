using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Permissions.Commands;
using IntelliPM.Application.Permissions.Queries;
using IntelliPM.Application.Permissions.DTOs;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using Microsoft.Extensions.Logging;
using IntelliPM.API.Authorization;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for managing member permissions (Admin only - own organization).
/// Provides endpoints for viewing and updating member roles and permissions.
/// </summary>
[ApiController]
[Route("api/admin/permissions")]
[ApiVersion("1.0")]
[RequireAdmin]
public class AdminMemberPermissionsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminMemberPermissionsController> _logger;

    public AdminMemberPermissionsController(
        IMediator mediator,
        ILogger<AdminMemberPermissionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get a paginated list of organization members with their permissions (Admin only).
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search term for filtering by name or email</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of members with permissions</returns>
    [HttpGet("members")]
    [ProducesResponseType(typeof(PagedResponse<MemberPermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMemberPermissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetMemberPermissionsQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get member permissions");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member permissions");
            return Problem(
                title: "Error retrieving member permissions",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update a member's role and/or permissions (Admin only - own organization).
    /// Enforces organization permission policy: assigned permissions must be subset of org allowed permissions.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Permission update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated member permission information</returns>
    [HttpPut("members/{userId}")]
    [ProducesResponseType(typeof(MemberPermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMemberPermission(
        [FromRoute] int userId,
        [FromBody] UpdateMemberPermissionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new UpdateMemberPermissionCommand
            {
                UserId = userId,
                GlobalRole = request.GlobalRole,
                PermissionIds = request.PermissionIds
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating member permissions for user {UserId}", userId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update member permissions for user {UserId}", userId);
            return Forbid();
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error updating member permissions for user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member permissions for user {UserId}", userId);
            return Problem(
                title: "Error updating member permissions",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

