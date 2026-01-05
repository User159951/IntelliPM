using IntelliPM.Domain.Interfaces;
using System.Text.Json;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Defines allowed permissions for an organization.
/// SuperAdmin can restrict which permissions are available to members of each organization.
/// If no policy exists for an organization, all permissions are allowed (default behavior).
/// </summary>
public class OrganizationPermissionPolicy : IAggregateRoot
{
    public int Id { get; set; }
    
    /// <summary>
    /// Organization ID (unique - one policy per organization).
    /// </summary>
    public int OrganizationId { get; set; }
    
    /// <summary>
    /// JSON array of allowed permission names (e.g., ["projects.create", "projects.edit", "users.view"]).
    /// If null or empty, all permissions are allowed (default behavior).
    /// </summary>
    public string AllowedPermissionsJson { get; set; } = "[]";
    
    /// <summary>
    /// Whether this policy is active. If false, all permissions are allowed (default behavior).
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Timestamp when this policy was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Timestamp when this policy was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
    
    /// <summary>
    /// Gets the list of allowed permissions from JSON.
    /// </summary>
    public List<string> GetAllowedPermissions()
    {
        if (string.IsNullOrWhiteSpace(AllowedPermissionsJson))
        {
            return new List<string>();
        }
        
        try
        {
            return JsonSerializer.Deserialize<List<string>>(AllowedPermissionsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
    
    /// <summary>
    /// Sets the list of allowed permissions as JSON.
    /// </summary>
    public void SetAllowedPermissions(List<string> permissions)
    {
        AllowedPermissionsJson = JsonSerializer.Serialize(permissions ?? new List<string>());
    }
    
    /// <summary>
    /// Checks if a permission is allowed by this policy.
    /// Returns true if policy is inactive, empty, or contains the permission.
    /// </summary>
    public bool IsPermissionAllowed(string permission)
    {
        if (!IsActive)
        {
            return true; // Inactive policy = allow all
        }
        
        var allowed = GetAllowedPermissions();
        if (allowed.Count == 0)
        {
            return true; // Empty policy = allow all (default)
        }
        
        return allowed.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}

