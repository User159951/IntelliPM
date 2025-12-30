namespace IntelliPM.Domain.Entities;

/// <summary>
/// Logs every agent execution for audit and metrics
/// </summary>
public class AgentExecutionLog
{
    public Guid Id { get; set; }
    public string AgentId { get; set; } = string.Empty; // e.g., "task-improver", "risk-analyzer"
    public string UserId { get; set; } = string.Empty;
    public string UserInput { get; set; } = string.Empty;
    public string? AgentResponse { get; set; }
    public string? ToolsCalled { get; set; } // Comma-separated list of tools called
    public string Status { get; set; } = "Pending"; // Pending, Success, Error
    public int ExecutionTimeMs { get; set; }
    public decimal ExecutionCostUsd { get; set; } // Always 0 for local LLM (llama3.2:3b)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}

