namespace IntelliPM.Application.Common.Interfaces;

public record DocumentVector(int Id, string Content, float[] Embedding, string Type, int ProjectId);

public interface IVectorStore
{
    Task StoreDocumentAsync(int projectId, string type, string content, float[] embedding, string? metadata, CancellationToken ct = default);
    Task<List<DocumentVector>> SearchAsync(int projectId, float[] queryEmbedding, int topK = 5, CancellationToken ct = default);
    Task<string> RetrieveContextAsync(int projectId, string queryType, int topK = 5, CancellationToken ct = default);
}

