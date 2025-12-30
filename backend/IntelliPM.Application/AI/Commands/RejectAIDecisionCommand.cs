using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to reject an AI decision.
/// </summary>
public record RejectAIDecisionCommand : IRequest
{
    public Guid DecisionId { get; init; }
    public string? RejectionNotes { get; init; }
    public string RejectionReason { get; init; } = string.Empty;
}

