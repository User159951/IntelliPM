namespace IntelliPM.Application.Abstractions.VectorStore;

/// <summary>
/// Result from vector similarity search
/// </summary>
public record MemorySearchResult(
    Guid Id,
    string Content,
    double Score, // Similarity score (0.0 to 1.0, higher is better)
    DateTimeOffset CreatedAt,
    string Type
);

