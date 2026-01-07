namespace IntelliPM.Application.Reports.DTOs;

/// <summary>
/// DTO for role-based activity reporting.
/// Shows actions grouped by user role.
/// </summary>
public record RoleActivityReportDto
{
    public string Role { get; init; } = string.Empty; // ProjectRole or GlobalRole
    public string ActionType { get; init; } = string.Empty; // task_created, task_updated, etc.
    public int Count { get; init; }
    public DateTimeOffset? LastPerformed { get; init; }
    public int UniqueUsers { get; init; } // Number of unique users with this role performing this action
}

