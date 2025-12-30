using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Domain.Entities;

public class RolePermission : IAggregateRoot
{
    public int Id { get; set; }
    public GlobalRole Role { get; set; }
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

