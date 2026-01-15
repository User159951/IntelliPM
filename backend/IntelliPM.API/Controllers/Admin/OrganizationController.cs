using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Organizations.Queries;
using IntelliPM.Application.Organizations.Commands;
using IntelliPM.Application.Organizations.DTOs;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Identity.DTOs;
using IntelliPM.Domain.Enums;
using Microsoft.Extensions.Logging;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Controller for managing the current user's organization (Admin only - own organization).
/// Provides endpoints for viewing organization details and managing members.
/// </summary>
[ApiController]
[Route("api/admin/organization")]
[ApiVersion("1.0")]
[RequireAdmin]
public class OrganizationController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrganizationController> _logger;

    public OrganizationController(
        IMediator mediator,
        ILogger<OrganizationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's organization details (Admin only).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Organization details</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyOrganization(CancellationToken ct = default)
    {
        try
        {
            var query = new GetMyOrganizationQuery();
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get own organization.");
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization not found for current user.");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting own organization.");
            return Problem(
                title: "Error retrieving organization",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get a paginated list of organization members (Admin only - own organization).
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search term for filtering by name or email</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of organization members</returns>
    [HttpGet("members")]
    [ProducesResponseType(typeof(PagedResponse<UserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrganizationMembers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetOrganizationMembersQuery
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
            _logger.LogWarning(ex, "Unauthorized attempt to get organization members.");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization members.");
            return Problem(
                title: "Error retrieving organization members",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get the current user's organization permission policy (Admin only).
    /// Returns the permission policy for the admin's organization.
    /// If no policy exists, returns default values (all permissions allowed).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Organization permission policy</returns>
    [HttpGet("permission-policy")]
    [ProducesResponseType(typeof(OrganizationPermissionPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyOrganizationPermissionPolicy(CancellationToken ct = default)
    {
        try
        {
            var query = new GetMyOrganizationPermissionPolicyQuery();
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to get own organization permission policy.");
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Organization not found for current user.");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting own organization permission policy.");
            return Problem(
                title: "Error retrieving organization permission policy",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update a user's global role within the organization (Admin only - own organization).
    /// Admin can only assign Admin or User roles, not SuperAdmin.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Role update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated user role</returns>
    [HttpPut("members/{userId}/global-role")]
    [ProducesResponseType(typeof(UpdateUserGlobalRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserGlobalRole(
        int userId,
        [FromBody] UpdateUserGlobalRoleRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (userId != request.UserId)
            {
                return BadRequest(new { message = "User ID in route must match User ID in body." });
            }

            var command = new UpdateUserGlobalRoleCommand
            {
                UserId = request.UserId,
                GlobalRole = request.GlobalRole
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update user {UserId} role.", userId);
            return Forbid();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found.", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating user {UserId} role.", userId);
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId} role.", userId);
            return Problem(
                title: "Error updating user role",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

/// <summary>
/// Request DTO for updating user global role.
/// </summary>
public record UpdateUserGlobalRoleRequest(
    int UserId,
    GlobalRole GlobalRole
);

