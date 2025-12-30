using Npgsql;
using NpgsqlTypes;
using IntelliPM.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.VectorStore;

public class PostgresVectorStore : IVectorStore
{
    private readonly string _connectionString;
    private readonly ILlmClient _llmClient;
    private readonly ILogger<PostgresVectorStore> _logger;

    public PostgresVectorStore(IConfiguration config, ILlmClient llmClient, ILogger<PostgresVectorStore> logger)
    {
        _connectionString = config.GetConnectionString("VectorDb") ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=intellipm_vector";
        _llmClient = llmClient;
        _logger = logger;
    }

    public async Task StoreDocumentAsync(int projectId, string type, string content, float[] embedding, string? metadata, CancellationToken ct = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = new NpgsqlCommand(
                @"INSERT INTO document_store (project_id, type, content, embedding, metadata, created_at)
                  VALUES (@projectId, @type, @content, @embedding, @metadata::jsonb, NOW())",
                conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.Add(new NpgsqlParameter("@embedding", new Pgvector.Vector(embedding)));
            cmd.Parameters.AddWithValue("@metadata", metadata ?? "{}");

            await cmd.ExecuteNonQueryAsync(ct);
            _logger.LogInformation($"Stored document in pgvector for project {projectId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing document in pgvector");
        }
    }

    public async Task<List<DocumentVector>> SearchAsync(int projectId, float[] queryEmbedding, int topK = 5, CancellationToken ct = default)
    {
        var results = new List<DocumentVector>();

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = new NpgsqlCommand(
                @"SELECT id, content, embedding, type, project_id
                  FROM document_store
                  WHERE project_id = @projectId
                  ORDER BY embedding <-> @embedding
                  LIMIT @topK",
                conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);
            cmd.Parameters.Add(new NpgsqlParameter("@embedding", new Pgvector.Vector(queryEmbedding)));
            cmd.Parameters.AddWithValue("@topK", topK);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                results.Add(new DocumentVector(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    (float[])reader.GetValue(2),
                    reader.GetString(3),
                    reader.GetInt32(4)
                ));
            }

            _logger.LogInformation($"Found {results.Count} similar documents");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching pgvector");
        }

        return results;
    }

    public async Task<string> RetrieveContextAsync(int projectId, string queryType, int topK = 5, CancellationToken ct = default)
    {
        try
        {
            var queryEmbedding = await _llmClient.GenerateEmbeddingAsync($"Project {projectId} {queryType} context", ct);
            var docs = await SearchAsync(projectId, queryEmbedding, topK, ct);

            return string.Join("\n\n", docs.Select(d => d.Content));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving context from pgvector");
            return "";
        }
    }
}

