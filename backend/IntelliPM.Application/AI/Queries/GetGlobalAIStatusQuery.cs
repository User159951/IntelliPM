using MediatR;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get the current status of the global AI kill switch.
/// </summary>
public record GetGlobalAIStatusQuery : IRequest<GlobalAIStatusResponse>;

/// <summary>
/// Response containing the current global AI status.
/// </summary>
public record GlobalAIStatusResponse(
    bool Enabled,
    DateTimeOffset? LastUpdated,
    int? UpdatedById,
    string? Reason
);
