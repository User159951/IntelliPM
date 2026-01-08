using IntelliPM.Domain.Interfaces;
using System.Text.Json;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Defines allowed permissions for an organization.
/// SuperAdmin can restrict which permissions are available to members of each organization.
/// SECURITY: Deny by default - permissions must be explicitly allowed in an active policy.
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
    /// SECURITY: If null or empty, no permissions are allowed (deny by default).
    /// </summary>
    public string AllowedPermissionsJson { get; set; } = "[]";
    
    /// <summary>
    /// Whether this policy is active. 
    /// SECURITY: If false, no permissions are allowed (deny by default).
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
    /// SECURITY: Deny by default - only returns true if policy is active AND explicitly contains the permission.
    /// </summary>
    public bool IsPermissionAllowed(string permission)
    {
        // Deny if policy is inactive
        if (!IsActive)
        {
            return false;
        }
        
        var allowed = GetAllowedPermissions();
        
        // Deny if policy is empty (no permissions explicitly allowed)
        if (allowed.Count == 0)
        {
            return false;
        }
        
        // Only allow if permission is explicitly in the allowed list
        return allowed.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}

