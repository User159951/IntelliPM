using MediatR;

namespace IntelliPM.Application.Admin.SystemHealth.Queries;

public record GetSystemHealthQuery() : IRequest<SystemHealthDto>;

public record SystemHealthDto(
    double CpuUsage,
    double MemoryUsage,
    long TotalMemoryBytes,
    long UsedMemoryBytes,
    long AvailableMemoryBytes,
    string DatabaseStatus,
    string DatabaseResponseTimeMs,
    Dictionary<string, ExternalServiceStatus> ExternalServices,
    int DeadLetterQueueCount,
    DateTime Timestamp
);

public record ExternalServiceStatus(
    string Name,
    bool IsHealthy,
    string? StatusMessage,
    int? ResponseTimeMs,
    DateTime? LastChecked
);

