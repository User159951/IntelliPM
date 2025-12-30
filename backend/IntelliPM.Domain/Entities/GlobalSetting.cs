using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public class GlobalSetting : IAggregateRoot
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General"; // General, Security, Email, FeatureFlags
    public int? UpdatedById { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation
    public User? UpdatedBy { get; set; }
}

