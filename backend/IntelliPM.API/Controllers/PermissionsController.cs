using Asp.Versioning;
using IntelliPM.API.Controllers;
using IntelliPM.Application.Permissions.Queries;
using IntelliPM.Application.Permissions.Commands;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class PermissionsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;

    public PermissionsController(
        IMediator mediator, 
        ICurrentUserService currentUserService,
        IPermissionService permissionService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get current user's permissions
    /// </summary>
    /// <remarks>
    /// Returns the current authenticated user's permissions and global role.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user's permissions and global role</returns>
    /// <response code="200">Permissions retrieved successfully</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserPermissionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var permissions = await _permissionService.GetUserPermissionsAsync(userId, cancellationToken);
            var globalRole = _currentUserService.GetGlobalRole();

            var response = new UserPermissionsResponse
            {
                Permissions = permissions.ToArray(),
                GlobalRole = globalRole
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Error retrieving user permissions",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get permissions matrix (Admin only)
    /// </summary>
    /// <remarks>
    /// Returns a matrix showing all permissions and which roles have access to them.
    /// Only administrators can access this endpoint.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Permissions matrix</returns>
    /// <response code="200">Permissions matrix retrieved successfully</response>
    /// <response code="403">Forbidden - User is not an administrator</response>
    [HttpGet("matrix")]
    [RequirePermission("admin.permissions.update")]
    [ProducesResponseType(typeof(PermissionsMatrixDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMatrix(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAdmin())
        {
            return Forbid();
        }

        var result = await _mediator.Send(new GetPermissionsMatrixQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update permissions for a role (Admin only)
    /// </summary>
    /// <remarks>
    /// Updates the permissions assigned to a specific role.
    /// Only administrators can modify role permissions.
    /// </remarks>
    /// <param name="role">Role name (Admin, Manager, Developer, Tester, Viewer)</param>
    /// <param name="request">List of permission IDs to assign to the role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Role permissions updated successfully</response>
    /// <response code="400">Bad request - Invalid role name</response>
    /// <response code="403">Forbidden - User is not an administrator</response>
    [HttpPut("roles/{role}")]
    [RequirePermission("admin.permissions.update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRolePermissions(
        string role,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAdmin())
        {
            return Forbid();
        }

        if (!Enum.TryParse<GlobalRole>(role, true, out var parsedRole))
        {
            return BadRequest(new { error = $"Invalid role '{role}'" });
        }

        var cmd = new UpdateRolePermissionsCommand(parsedRole, request.PermissionIds ?? new List<int>());
        await _mediator.Send(cmd, cancellationToken);
        return NoContent();
    }
}

public record UpdateRolePermissionsRequest(List<int> PermissionIds);

public class UserPermissionsResponse
{
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public GlobalRole GlobalRole { get; set; }
}

