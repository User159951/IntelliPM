using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to immediately disable all AI features for an organization.
/// Emergency kill switch for AI features per organization.
/// </summary>
public record DisableAIForOrgCommand : IRequest<DisableAIForOrgResponse>
{
    public int OrganizationId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public bool NotifyOrganization { get; init; } = true;
    public DisableMode Mode { get; init; } = DisableMode.Temporary;
}

/// <summary>
/// Mode for disabling AI features.
/// </summary>
public enum DisableMode
{
    /// <summary>
    /// Temporary disable - can be re-enabled by admin.
    /// </summary>
    Temporary,

    /// <summary>
    /// Permanent disable - requires manual intervention.
    /// </summary>
    Permanent
}

/// <summary>
/// Response containing details about the AI disable operation.
/// </summary>
public record DisableAIForOrgResponse(
    int OrganizationId,
    string OrganizationName,
    bool WasDisabled,
    DisableMode Mode,
    DateTimeOffset DisabledAt,
    string Reason
);

