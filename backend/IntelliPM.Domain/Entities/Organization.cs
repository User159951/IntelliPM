using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public class Organization : IAggregateRoot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}

