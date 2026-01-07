namespace IntelliPM.Application.Reports.DTOs;

/// <summary>
/// DTO for workflow transition reporting by role.
/// Shows status changes grouped by user role.
/// </summary>
public record WorkflowRoleReportDto
{
    public string Role { get; init; } = string.Empty; // ProjectRole or GlobalRole
    public string FromStatus { get; init; } = string.Empty; // Previous status
    public string ToStatus { get; init; } = string.Empty; // New status
    public string EntityType { get; init; } = string.Empty; // task, sprint, project, etc.
    public int TransitionCount { get; init; }
    public DateTimeOffset? LastTransition { get; init; }
    public int UniqueUsers { get; init; } // Number of unique users with this role making this transition
}

