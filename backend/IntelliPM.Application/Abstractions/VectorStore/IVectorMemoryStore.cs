namespace IntelliPM.Application.Abstractions.VectorStore;

/// <summary>
/// Interface for vector-based agent memory storage using pgvector
/// </summary>
public interface IVectorMemoryStore
{
    /// <summary>
    /// Store a note/document with its embedding for a project
    /// </summary>
    Task AddNoteAsync(
        Guid projectId, 
        string type, 
        string content, 
        float[] embedding, 
        CancellationToken ct = default);

    /// <summary>
    /// Search for similar memories using vector similarity (cosine distance)
    /// </summary>
    Task<IReadOnlyList<MemorySearchResult>> SearchAsync(
        Guid projectId, 
        float[] embedding, 
        int topK = 5, 
        CancellationToken ct = default);

    /// <summary>
    /// Retrieve all memories for a project by type
    /// </summary>
    Task<IReadOnlyList<MemorySearchResult>> GetByTypeAsync(
        Guid projectId, 
        string type, 
        int limit = 10, 
        CancellationToken ct = default);
}

