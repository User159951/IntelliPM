using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Enums;
using System.Text.Json;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Logs AI decision-making process for explainability, audit trail, and governance.
/// Separate from AgentExecutionLog which tracks execution lifecycle.
/// Stores complete decision context, reasoning chain, and outcomes.
/// </summary>
public class AIDecisionLog : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy

    // Decision identification
    public Guid DecisionId { get; set; } = Guid.NewGuid(); // Unique decision identifier
    public string DecisionType { get; set; } = string.Empty; // "RiskDetection", "SprintPlanning", "TaskPrioritization", etc.
    public string AgentType { get; set; } = string.Empty; // "DeliveryAgent", "ProductAgent", etc.

    // Context
    public string EntityType { get; set; } = string.Empty; // "Project", "Sprint", "Task"
    public int EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty; // Denormalized for quick reference

    // Decision details
    public string Question { get; set; } = string.Empty; // What was asked
    public string Decision { get; set; } = string.Empty; // What was decided (JSON or text)
    public string Reasoning { get; set; } = string.Empty; // Why this decision (JSON with reasoning chain)
    public decimal ConfidenceScore { get; set; } // 0.0 to 1.0

    // AI Model information
    public string ModelName { get; set; } = string.Empty; // "llama3.2:3b", "gpt-4", etc.
    public string ModelVersion { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }

    // Input/Output
    public string InputData { get; set; } = string.Empty; // JSON of input context
    public string OutputData { get; set; } = string.Empty; // JSON of full output
    public string AlternativesConsidered { get; set; } = "[]"; // JSON array of alternatives

    // Human oversight
    public int RequestedByUserId { get; set; }
    public bool RequiresHumanApproval { get; set; } = false;
    public bool? ApprovedByHuman { get; set; } // null = pending, true = approved, false = rejected
    public int? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    
    // Rejection tracking
    public int? RejectedByUserId { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    
    // Approval deadline (decisions expire after 48h if not approved)
    public DateTimeOffset? ApprovalDeadline { get; set; }

    // Outcome tracking
    public AIDecisionStatus Status { get; set; } = AIDecisionStatus.Pending;
    
    // Legacy string status for backward compatibility (stored in DB as string, mapped to enum)
    [Obsolete("Use Status enum property instead")]
    public string StatusString 
    { 
        get => Status.ToString(); 
        set => Status = Enum.TryParse<AIDecisionStatus>(value, true, out var parsed) ? parsed : AIDecisionStatus.Pending;
    }
    public bool WasApplied { get; set; } = false;
    public DateTimeOffset? AppliedAt { get; set; }
    public string? ActualOutcome { get; set; } // What actually happened after decision

    // Cost tracking
    public decimal CostAccumulated { get; set; } = 0m; // Cost in USD for this decision

    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public int ExecutionTimeMs { get; set; } // How long decision took
    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string ExecutionStatus { get; set; } = "Success"; // "Success", "Failed", "PartialFailure"
    public string? CorrelationId { get; set; } // Request correlation ID for distributed tracing

    // Navigation properties
    public User RequestedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public User? RejectedByUser { get; set; }

    // Helper methods for JSON serialization
    public List<AlternativeDecision> GetAlternativesConsidered()
    {
        return JsonSerializer.Deserialize<List<AlternativeDecision>>(AlternativesConsidered) ?? new();
    }

    public void SetAlternativesConsidered(List<AlternativeDecision> alternatives)
    {
        AlternativesConsidered = JsonSerializer.Serialize(alternatives);
    }

    public T GetInputDataAs<T>() where T : class
    {
        return JsonSerializer.Deserialize<T>(InputData) ?? throw new InvalidOperationException("Failed to deserialize input data");
    }

    public void SetInputData<T>(T data) where T : class
    {
        InputData = JsonSerializer.Serialize(data);
    }

    public T GetOutputDataAs<T>() where T : class
    {
        return JsonSerializer.Deserialize<T>(OutputData) ?? throw new InvalidOperationException("Failed to deserialize output data");
    }

    public void SetOutputData<T>(T data) where T : class
    {
        OutputData = JsonSerializer.Serialize(data);
    }

    public void ApproveDecision(int approvedByUserId, string? notes = null)
    {
        ApprovedByHuman = true;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTimeOffset.UtcNow;
        ApprovalNotes = notes;
        Status = AIDecisionStatus.Approved;
        
        // Clear rejection fields if previously rejected
        RejectedByUserId = null;
        RejectedAt = null;
    }

    public void RejectDecision(int rejectedByUserId, string? notes = null)
    {
        ApprovedByHuman = false;
        RejectedByUserId = rejectedByUserId;
        RejectedAt = DateTimeOffset.UtcNow;
        ApprovalNotes = notes;
        Status = AIDecisionStatus.Rejected;
        
        // Clear approval fields if previously approved
        ApprovedByUserId = null;
        ApprovedAt = null;
    }
    
    /// <summary>
    /// Marks the decision as applied/executed.
    /// </summary>
    public void MarkAsApplied()
    {
        Status = AIDecisionStatus.Applied;
        WasApplied = true;
        AppliedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Marks the decision as expired (past approval deadline).
    /// </summary>
    public void MarkAsExpired()
    {
        Status = AIDecisionStatus.Expired;
    }
    
    /// <summary>
    /// Checks if the decision has expired (past approval deadline).
    /// </summary>
    public bool IsExpired()
    {
        if (!ApprovalDeadline.HasValue)
            return false;
            
        return DateTimeOffset.UtcNow > ApprovalDeadline.Value;
    }
    
    /// <summary>
    /// Checks if the decision can be executed (approved and not expired).
    /// </summary>
    public bool CanBeExecuted()
    {
        if (!RequiresHumanApproval)
            return true; // No approval required
            
        if (IsExpired())
            return false; // Expired
            
        return Status == AIDecisionStatus.Approved;
    }
    
    /// <summary>
    /// Sets the approval deadline to 48 hours from now.
    /// </summary>
    public void SetApprovalDeadline()
    {
        ApprovalDeadline = DateTimeOffset.UtcNow.AddHours(48);
    }
}

/// <summary>
/// Represents an alternative decision option that was considered.
/// </summary>
public class AlternativeDecision
{
    public string Option { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public decimal Score { get; set; }
}

