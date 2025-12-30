namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for evaluating user permissions based on their GlobalRole
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets all permissions for a user based on their GlobalRole
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of permission names</returns>
    Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user has a specific permission
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="permission">The permission name to check (e.g., "projects.create")</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if user has the permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken ct = default);
}

