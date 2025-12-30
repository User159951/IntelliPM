namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for accessing current user information from the HTTP context
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <returns>User ID or 0 if not found</returns>
    int GetUserId();

    /// <summary>
    /// Gets the current user's organization ID
    /// </summary>
    /// <returns>Organization ID or 0 if not found</returns>
    int GetOrganizationId();

    /// <summary>
    /// Checks if the current user is an admin
    /// </summary>
    /// <returns>True if user has "Admin" role, false otherwise</returns>
    bool IsAdmin();
}

