using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IntelliPM.Application.Abstractions.VectorStore;
using IntelliPM.Infrastructure.VectorStore.Entities;
using Pgvector;

namespace IntelliPM.Infrastructure.VectorStore;

/// <summary>
/// PostgreSQL + pgvector implementation of vector memory store
/// </summary>
public class VectorMemoryStorePgvector : IVectorMemoryStore
{
    private readonly VectorDbContext _context;
    private readonly ILogger<VectorMemoryStorePgvector> _logger;

    public VectorMemoryStorePgvector(VectorDbContext context, ILogger<VectorMemoryStorePgvector> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddNoteAsync(
        Guid projectId, 
        string type, 
        string content, 
        float[] embedding, 
        CancellationToken ct = default)
    {
        try
        {
            var record = new AgentMemoryRecord
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Type = type,
                Content = content,
                Embedding = new Vector(embedding),
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.AgentMemories.Add(record);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Stored agent memory: ProjectId={ProjectId}, Type={Type}, RecordId={RecordId}", 
                projectId, type, record.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing agent memory for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<IReadOnlyList<MemorySearchResult>> SearchAsync(
        Guid projectId, 
        float[] embedding, 
        int topK = 5, 
        CancellationToken ct = default)
    {
        try
        {
            // Fetch all records for the project (TODO: optimize for large datasets with SQL)
            var records = await _context.AgentMemories
                .Where(m => m.ProjectId == projectId)
                .ToListAsync(ct);

            if (!records.Any())
            {
                return Array.Empty<MemorySearchResult>();
            }

            // Calculate cosine similarity in memory
            var results = records
                .Select(record => new
                {
                    Record = record,
                    Similarity = CosineSimilarity(embedding, record.Embedding.ToArray())
                })
                .OrderByDescending(x => x.Similarity)
                .Take(topK)
                .Select(x => new MemorySearchResult(
                    x.Record.Id,
                    x.Record.Content,
                    x.Similarity,
                    x.Record.CreatedAt,
                    x.Record.Type
                ))
                .ToList();

            _logger.LogInformation(
                "Vector search completed: ProjectId={ProjectId}, TopK={TopK}, ResultCount={Count}", 
                projectId, topK, results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing vector search for project {ProjectId}", projectId);
            return Array.Empty<MemorySearchResult>();
        }
    }

    private static double CosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            return 0.0;

        double dotProduct = 0.0;
        double magnitude1 = 0.0;
        double magnitude2 = 0.0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        if (magnitude1 == 0.0 || magnitude2 == 0.0)
            return 0.0;

        return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
    }

    public async Task<IReadOnlyList<MemorySearchResult>> GetByTypeAsync(
        Guid projectId, 
        string type, 
        int limit = 10, 
        CancellationToken ct = default)
    {
        try
        {
            var results = await _context.AgentMemories
                .Where(m => m.ProjectId == projectId && m.Type == type)
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .Select(m => new MemorySearchResult(
                    m.Id,
                    m.Content,
                    1.0, // Score - no similarity calculation, just retrieval
                    m.CreatedAt,
                    m.Type
                ))
                .ToListAsync(ct);

            _logger.LogInformation(
                "Retrieved memories by type: ProjectId={ProjectId}, Type={Type}, Count={Count}", 
                projectId, type, results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memories by type for project {ProjectId}", projectId);
            return Array.Empty<MemorySearchResult>();
        }
    }
}

