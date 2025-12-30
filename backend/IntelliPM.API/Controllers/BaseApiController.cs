using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers
/// </summary>
[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <returns>Current user ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID is not found in claims</exception>
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }

    /// <summary>
    /// Gets the current user's username from JWT claims
    /// </summary>
    /// <returns>Current user's username or null if not found</returns>
    protected string? GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Gets the current user's email from JWT claims
    /// </summary>
    /// <returns>Current user's email or null if not found</returns>
    protected string? GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    /// <param name="role">Role name to check</param>
    /// <returns>True if user has the role, false otherwise</returns>
    protected bool HasRole(string role)
    {
        return User.IsInRole(role);
    }

    /// <summary>
    /// Gets the current user's organization ID from JWT claims
    /// </summary>
    /// <returns>Organization ID or 0 if not found</returns>
    protected int GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId");
        if (claim != null && int.TryParse(claim.Value, out var organizationId))
        {
            return organizationId;
        }
        return 0;
    }
}

