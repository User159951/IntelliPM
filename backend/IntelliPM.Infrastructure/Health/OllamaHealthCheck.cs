using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace IntelliPM.Infrastructure.Health;

public class OllamaHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public OllamaHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var ollamaEndpoint = _configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5); // Reasonable timeout for health checks

            var response = await client.GetAsync($"{ollamaEndpoint}/api/tags", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Ollama is running and accessible");
            }

            // Return Degraded instead of Unhealthy - Ollama is optional for API functionality
            return HealthCheckResult.Degraded($"Ollama returned status code: {response.StatusCode}");
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is TimeoutException)
        {
            // Return Degraded instead of Unhealthy - connection failures mean Ollama isn't available but API can still work
            return HealthCheckResult.Degraded("Ollama is not accessible (AI features unavailable)", ex);
        }
        catch (Exception ex)
        {
            // Only return Unhealthy for unexpected errors
            return HealthCheckResult.Unhealthy("Ollama health check failed with unexpected error", ex);
        }
    }
}

