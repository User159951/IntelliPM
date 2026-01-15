using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Organization entity for multi-tenancy support.
/// Each organization represents a tenant in the system.
/// </summary>
public class Organization : IAggregateRoot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique code/slug for the organization (e.g., "acme-corp", "default").
    /// Used for URL-friendly identification and API access.
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization's default language (ISO 639-1: en, fr, ar).
    /// Used as fallback when user doesn't have a preferred language.
    /// Defaults to 'en' if not set.
    /// </summary>
    public string? DefaultLanguage { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}

