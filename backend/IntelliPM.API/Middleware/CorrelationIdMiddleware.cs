using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IntelliPM.API.Middleware;

/// <summary>
/// Middleware to extract or generate correlation ID for request tracing.
/// Extracts correlation ID from X-Correlation-ID header, or generates a new Guid if not present.
/// Stores the correlation ID in HttpContext.Items for use throughout the request pipeline.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    public const string CorrelationIdItemKey = "CorrelationId";

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract correlation ID from header or generate new one
        string correlationId;
        
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue.ToString()))
        {
            correlationId = headerValue.ToString()!;
        }
        else
        {
            // Generate new correlation ID
            correlationId = Guid.NewGuid().ToString();
        }

        // Store in HttpContext.Items for access throughout the request
        context.Items[CorrelationIdItemKey] = correlationId;

        // Add to response header so clients can track it
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        // Add to Serilog LogContext for automatic inclusion in all logs
        // PushProperty returns a disposable that will clean up when disposed
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

