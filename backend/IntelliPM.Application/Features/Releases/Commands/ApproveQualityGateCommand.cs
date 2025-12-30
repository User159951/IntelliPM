using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to manually approve a quality gate for a release.
/// Used for ManualApproval type quality gates.
/// </summary>
public record ApproveQualityGateCommand : IRequest<Unit>
{
    /// <summary>
    /// ID of the release.
    /// </summary>
    public int ReleaseId { get; init; }

    /// <summary>
    /// Type of quality gate to approve.
    /// </summary>
    public IntelliPM.Domain.Enums.QualityGateType GateType { get; init; }
}

