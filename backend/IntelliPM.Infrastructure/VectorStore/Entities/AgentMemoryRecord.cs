using Pgvector;

namespace IntelliPM.Infrastructure.VectorStore.Entities;

/// <summary>
/// Agent memory record stored in PostgreSQL with pgvector
/// </summary>
public class AgentMemoryRecord
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Type { get; set; } = string.Empty; // e.g., "note", "decision", "summary"
    public string Content { get; set; } = string.Empty;
    public Vector Embedding { get; set; } = null!; // Pgvector type
    public DateTimeOffset CreatedAt { get; set; }
    public string? Metadata { get; set; } // JSON metadata
}

