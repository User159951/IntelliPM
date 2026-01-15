using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Domain.Entities;

public class User : IAggregateRoot
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public GlobalRole GlobalRole { get; set; } = GlobalRole.User;
    public bool IsActive { get; set; } = true;
    public int OrganizationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    
    /// <summary>
    /// User's preferred language (ISO 639-1: en, fr, ar).
    /// Null means use organization default or browser language.
    /// </summary>
    public string? PreferredLanguage { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}

