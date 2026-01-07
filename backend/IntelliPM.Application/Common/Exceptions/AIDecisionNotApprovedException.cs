namespace IntelliPM.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when attempting to execute an AI decision that requires human approval but hasn't been approved yet.
/// </summary>
public class AIDecisionNotApprovedException : ApplicationException
{
    /// <summary>
    /// Decision ID that requires approval.
    /// </summary>
    public Guid DecisionId { get; }

    /// <summary>
    /// Decision type.
    /// </summary>
    public string DecisionType { get; }

    /// <summary>
    /// Current status of the decision.
    /// </summary>
    public string Status { get; }

    /// <summary>
    /// Organization ID that owns the decision.
    /// </summary>
    public int OrganizationId { get; }

    public AIDecisionNotApprovedException(
        Guid decisionId,
        string decisionType,
        string status,
        int organizationId,
        string? message = null)
        : base(message ?? $"AI decision {decisionId} ({decisionType}) requires approval but current status is {status}")
    {
        DecisionId = decisionId;
        DecisionType = decisionType;
        Status = status;
        OrganizationId = organizationId;
    }
}

