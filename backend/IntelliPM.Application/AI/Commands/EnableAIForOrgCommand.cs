using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to re-enable AI features for an organization.
/// Reverse operation of DisableAIForOrgCommand.
/// </summary>
public record EnableAIForOrgCommand : IRequest<EnableAIForOrgResponse>
{
    public int OrganizationId { get; init; }
    public string TierName { get; init; } = "Free";
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Response containing details about the AI enable operation.
/// </summary>
public record EnableAIForOrgResponse(
    int OrganizationId,
    string OrganizationName,
    bool WasEnabled,
    string TierName
);

