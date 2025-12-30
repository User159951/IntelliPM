using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public class Insight : IAggregateRoot
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int? AgentRunId { get; set; }
    public string AgentType { get; set; } = string.Empty; // Product | Delivery | QA | Business
    public string Category { get; set; } = string.Empty; // Risk | Opportunity | Warning | Info
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public decimal Confidence { get; set; } // 0.0 - 1.0
    public string Priority { get; set; } = "Medium"; // Low | Medium | High | Critical
    public string Status { get; set; } = "New"; // New | Acknowledged | Implemented | Dismissed
    public int? AcknowledgedById { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public string? UserFeedback { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Navigation
    public Project Project { get; set; } = null!;
    public AIAgentRun? AgentRun { get; set; }
    public User? AcknowledgedBy { get; set; }
}

