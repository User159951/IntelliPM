namespace IntelliPM.Application.Common.Interfaces;

public interface ILlmClient
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct = default);
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
}

