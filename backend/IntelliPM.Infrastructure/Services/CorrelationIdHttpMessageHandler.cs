using Microsoft.AspNetCore.Http;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// HTTP message handler that automatically adds the correlation ID header to outgoing HTTP requests.
/// This enables distributed tracing across services.
/// </summary>
public class CorrelationIdHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    // Constants matching CorrelationIdMiddleware in API project
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdItemKey = "CorrelationId";

    public CorrelationIdHttpMessageHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Get correlation ID from HttpContext.Items (set by CorrelationIdMiddleware)
            if (httpContext.Items.TryGetValue(CorrelationIdItemKey, out var correlationIdObj) &&
                correlationIdObj is string correlationId &&
                !string.IsNullOrWhiteSpace(correlationId))
            {
                // Add correlation ID header if not already present
                if (!request.Headers.Contains(CorrelationIdHeaderName))
                {
                    request.Headers.Add(CorrelationIdHeaderName, correlationId);
                }
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}

