using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to approve an AI decision (human-in-the-loop).
/// </summary>
public record ApproveAIDecisionCommand : IRequest
{
    public Guid DecisionId { get; init; }
    public string? ApprovalNotes { get; init; }
}

