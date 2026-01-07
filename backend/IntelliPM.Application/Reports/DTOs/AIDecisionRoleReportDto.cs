namespace IntelliPM.Application.Reports.DTOs;

/// <summary>
/// DTO for AI decision reporting by approver role.
/// Shows AI decisions grouped by the role of the user who approved/rejected them.
/// </summary>
public record AIDecisionRoleReportDto
{
    public string Role { get; init; } = string.Empty; // ProjectRole or GlobalRole of approver
    public int DecisionsApproved { get; init; }
    public int DecisionsRejected { get; init; }
    public int DecisionsPending { get; init; }
    public double AverageResponseTimeHours { get; init; } // Average time from creation to approval/rejection
    public int UniqueApprovers { get; init; } // Number of unique users with this role who approved/rejected
    public decimal AverageConfidenceScore { get; init; } // Average confidence score of decisions handled by this role
}

