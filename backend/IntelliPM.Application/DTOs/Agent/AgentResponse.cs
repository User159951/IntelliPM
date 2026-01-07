namespace IntelliPM.Application.DTOs.Agent;

/// <summary>
/// Response DTO for AI Agent operations
/// </summary>
public class AgentResponse
{
    /// <summary>
    /// The main content/result returned by the agent
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Status of the agent execution: Success, Error, ProposalReady
    /// </summary>
    public string Status { get; set; } = "Success";

    /// <summary>
    /// Indicates if the agent's suggestion requires human approval before execution
    /// </summary>
    public bool RequiresApproval { get; set; }

    /// <summary>
    /// Estimated or actual cost of the agent execution in USD
    /// </summary>
    public decimal ExecutionCostUsd { get; set; }

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public int ExecutionTimeMs { get; set; }

    /// <summary>
    /// List of tools/plugins called during agent execution
    /// </summary>
    public List<string> ToolsCalled { get; set; } = new();

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional error message if Status is "Error"
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Metadata for additional context (optional)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Number of prompt tokens used (input)
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of completion tokens used (output)
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total number of tokens used (prompt + completion)
    /// </summary>
    public int TotalTokens => PromptTokens + CompletionTokens;

    /// <summary>
    /// Model name used for this response (e.g., "gpt-4", "llama3.2:3b")
    /// </summary>
    public string Model { get; set; } = string.Empty;
}

