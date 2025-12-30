using IntelliPM.Application.Features.Releases.DTOs;
using MediatR;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Command to evaluate all quality gates for a release.
/// Returns the list of quality gate evaluation results.
/// </summary>
public record EvaluateQualityGatesCommand : IRequest<List<QualityGateDto>>
{
    /// <summary>
    /// ID of the release to evaluate quality gates for.
    /// </summary>
    public int ReleaseId { get; init; }
}

