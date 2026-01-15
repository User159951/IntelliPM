using IntelliPM.Domain.Enums;

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
    /// Gets the current user's global role
    /// </summary>
    /// <returns>GlobalRole enum value, or User if not found</returns>
    GlobalRole GetGlobalRole();

    /// <summary>
    /// Checks if the current user is an admin (Admin or SuperAdmin)
    /// </summary>
    /// <returns>True if user has "Admin" or "SuperAdmin" role, false otherwise</returns>
    bool IsAdmin();

    /// <summary>
    /// Checks if the current user is a SuperAdmin
    /// </summary>
    /// <returns>True if user has "SuperAdmin" role, false otherwise</returns>
    bool IsSuperAdmin();

    /// <summary>
    /// Gets the correlation ID for the current request.
    /// </summary>
    /// <returns>Correlation ID or null if not available</returns>
    string? GetCorrelationId();
}

