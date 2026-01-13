using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Entity for tracking administrative actions and system changes for audit purposes
/// </summary>
public class AuditLog : IAggregateRoot, ITenantEntity
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // create, update, delete, login, etc.
    public string EntityType { get; set; } = string.Empty; // User, Project, Setting, etc.
    public int? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? Changes { get; set; } // JSON string with old/new values
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public User? User { get; set; }
    public Organization Organization { get; set; } = null!;
}

