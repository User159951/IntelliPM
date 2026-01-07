using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Defines approval policies for different types of AI decisions.
/// Determines which role is required to approve specific decision types and whether approval is blocking.
/// </summary>
public class AIDecisionApprovalPolicy : IAggregateRoot
{
    public int Id { get; set; }
    
    /// <summary>
    /// Organization ID (null = global/system-wide policy).
    /// </summary>
    public int? OrganizationId { get; set; }
    
    /// <summary>
    /// Decision type this policy applies to (e.g., "RiskDetection", "SprintPlanning", "TaskPrioritization", "CostDecision", "QuotaDecision", "CriticalSystemDecision").
    /// </summary>
    public string DecisionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Required role to approve this decision type (e.g., "ProductOwner", "Admin", "SuperAdmin").
    /// </summary>
    public string RequiredRole { get; set; } = string.Empty;
    
    /// <summary>
    /// If true, decisions of this type cannot be executed until approved.
    /// If false, decisions can proceed but approval is still tracked.
    /// </summary>
    public bool IsBlockingIfNotApproved { get; set; } = true;
    
    /// <summary>
    /// Whether this policy is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional description of the policy.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Timestamp when this policy was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Timestamp when this policy was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation properties
    public Organization? Organization { get; set; }
}

