using IntelliPM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Implementation of ICorrelationIdService that extracts correlation ID from HttpContext.
/// </summary>
public class CorrelationIdService : ICorrelationIdService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    // Constant matching CorrelationIdMiddleware in API project
    private const string CorrelationIdItemKey = "CorrelationId";

    public CorrelationIdService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string? GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        // Try to get from HttpContext.Items (set by CorrelationIdMiddleware)
        if (httpContext.Items.TryGetValue(CorrelationIdItemKey, out var correlationIdObj) &&
            correlationIdObj is string correlationId)
        {
            return correlationId;
        }

        return null;
    }
}

