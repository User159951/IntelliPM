using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to reset (delete) a user AI quota override (Admin only).
/// </summary>
public record ResetUserAIQuotaOverrideCommand : IRequest<ResetUserAIQuotaOverrideResponse>
{
    public int UserId { get; init; }
}

/// <summary>
/// Response for resetting user AI quota override.
/// </summary>
public record ResetUserAIQuotaOverrideResponse(
    int UserId,
    bool Success,
    string Message
);

