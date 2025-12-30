namespace IntelliPM.Domain.Entities;

public class DocumentStore
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Type { get; set; } = string.Empty; // "Note", "Decision", "Meeting"
    public string Content { get; set; } = string.Empty;
    public string? Metadata { get; set; } // JSON
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Project Project { get; set; } = null!;
}

public class AIAgentRun
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string AgentType { get; set; } = string.Empty; // "Product", "Delivery", "Manager"
    public string InputData { get; set; } = string.Empty; // JSON
    public string OutputData { get; set; } = string.Empty; // JSON
    public decimal? Confidence { get; set; }
    public DateTimeOffset ExecutedAt { get; set; } = DateTimeOffset.UtcNow;

    public Project Project { get; set; } = null!;
    public ICollection<AIDecision> Decisions { get; set; } = new List<AIDecision>();
}

public class AIDecision
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int AgentRunId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public DateTimeOffset? ExecutedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Project Project { get; set; } = null!;
    public AIAgentRun Run { get; set; } = null!;
}

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
}

