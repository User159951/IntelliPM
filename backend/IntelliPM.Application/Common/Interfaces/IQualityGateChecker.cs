using IntelliPM.Domain.ValueObjects;

namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for evaluating quality gates for releases.
/// Validates release readiness before deployment.
/// </summary>
public interface IQualityGateChecker
{
    /// <summary>
    /// Evaluates a specific quality gate for a release.
    /// </summary>
    /// <param name="releaseId">ID of the release to evaluate.</param>
    /// <param name="gateType">Type of quality gate to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>QualityGateResult representing the evaluation result.</returns>
    System.Threading.Tasks.Task<QualityGateResult> EvaluateQualityGateAsync(
        int releaseId,
        IntelliPM.Domain.Enums.QualityGateType gateType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Evaluates all quality gates for a release.
    /// </summary>
    /// <param name="releaseId">ID of the release to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of QualityGateResult for each evaluated gate.</returns>
    System.Threading.Tasks.Task<List<QualityGateResult>> EvaluateAllQualityGatesAsync(
        int releaseId,
        CancellationToken cancellationToken);
}

