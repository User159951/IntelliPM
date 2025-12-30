using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Domain.Entities;

public class Invitation : IAggregateRoot
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public GlobalRole GlobalRole { get; set; } = GlobalRole.User;
    public int OrganizationId { get; set; }
    public int? ProjectId { get; set; }
    public int CreatedById { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTimeOffset? UsedAt { get; set; }

    // Navigation
    public User CreatedBy { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public Project? Project { get; set; }
}

