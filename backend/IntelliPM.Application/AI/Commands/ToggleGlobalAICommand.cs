using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to toggle global AI kill switch (system-wide).
/// Emergency kill switch that affects all organizations.
/// </summary>
public record ToggleGlobalAICommand : IRequest<ToggleGlobalAIResponse>
{
    public bool Enabled { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Response containing details about the global AI toggle operation.
/// </summary>
public record ToggleGlobalAIResponse(
    bool Enabled,
    DateTimeOffset ToggledAt,
    string Reason,
    int UpdatedById
);
