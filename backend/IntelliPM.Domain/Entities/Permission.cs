using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public class Permission : IAggregateRoot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "projects.create"
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty; // e.g., "Projects", "Users", "Admin"
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

