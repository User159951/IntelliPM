using IntelliPM.Application.Common.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.LLM;

public class OllamaClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaClient> _logger;

    public OllamaClient(HttpClient httpClient, ILogger<OllamaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GenerateTextAsync(string prompt, CancellationToken ct = default)
    {
        try
        {
            var request = new { prompt, model = "llama2", stream = false };
            var response = await _httpClient.PostAsJsonAsync("/api/generate", request, cancellationToken: ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: ct);
            _logger.LogInformation("Ollama generated text successfully");
            return result?.Response ?? "No response";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama for text generation");
            return "Error generating response";
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var request = new { prompt = text, model = "llama2" };
            var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, cancellationToken: ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: ct);
            _logger.LogInformation("Ollama generated embedding successfully");
            return result?.Embedding ?? new float[1536];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama for embeddings");
            return new float[1536]; // Return zero vector on error
        }
    }
}

public record OllamaGenerateResponse(string Response);
public record OllamaEmbeddingResponse(float[] Embedding);

