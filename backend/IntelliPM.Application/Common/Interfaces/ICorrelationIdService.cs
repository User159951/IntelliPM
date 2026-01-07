namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service to access the correlation ID for the current request.
/// </summary>
public interface ICorrelationIdService
{
    /// <summary>
    /// Gets the correlation ID for the current request, or null if not available.
    /// </summary>
    string? GetCorrelationId();
}

