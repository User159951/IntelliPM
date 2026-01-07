namespace IntelliPM.Domain.Entities;

/// <summary>
/// Logs every agent execution for audit and metrics
/// </summary>
public class AgentExecutionLog
{
    public Guid Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy: Organization that owns this execution log
    public string AgentId { get; set; } = string.Empty; // e.g., "task-improver", "risk-analyzer"
    public string AgentType { get; set; } = string.Empty; // e.g., "DeliveryAgent", "ProductAgent"
    public string UserId { get; set; } = string.Empty;
    public string UserInput { get; set; } = string.Empty;
    public string? AgentResponse { get; set; }
    public string? ToolsCalled { get; set; } // Comma-separated list of tools called
    public string Status { get; set; } = "Pending"; // Pending, Success, Error
    public bool Success { get; set; } // True if execution succeeded, false if failed
    public int ExecutionTimeMs { get; set; }
    public int TokensUsed { get; set; } // Total tokens used in this execution
    public decimal ExecutionCostUsd { get; set; } // Always 0 for local LLM (llama3.2:3b)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
    
    // Foreign key to AIDecisionLog
    public int? LinkedDecisionId { get; set; }
    public AIDecisionLog? LinkedDecision { get; set; }
    
    // Navigation property
    public Organization? Organization { get; set; }
}

