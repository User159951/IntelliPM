using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for evaluating quality gates for releases.
/// Validates release readiness before deployment by checking various quality criteria.
/// </summary>
public class QualityGateChecker : IQualityGateChecker
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QualityGateChecker> _logger;

    public QualityGateChecker(
        IUnitOfWork unitOfWork,
        ILogger<QualityGateChecker> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<QualityGateResult> EvaluateQualityGateAsync(
        int releaseId,
        QualityGateType gateType,
        CancellationToken cancellationToken)
    {
        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.Sprints)
                .ThenInclude(s => s.Items)
                .ThenInclude(si => si.UserStory)
                .ThenInclude(us => us.Tasks)
            .Include(r => r.QualityGates)
                .ThenInclude(qg => qg.CheckedByUser)
            .FirstOrDefaultAsync(r => r.Id == releaseId, cancellationToken);

        if (release == null)
        {
            throw new NotFoundException($"Release {releaseId} not found");
        }

        return gateType switch
        {
            QualityGateType.AllTasksCompleted => await CheckAllTasksCompleted(release),
            QualityGateType.NoOpenBugs => await CheckNoOpenBugs(release),
            QualityGateType.ManualApproval => CheckManualApproval(release),
            QualityGateType.CodeCoverage => QualityGateResult.Pending(QualityGateType.CodeCoverage, "Code coverage check not yet implemented"),
            QualityGateType.CodeReviewApproval => QualityGateResult.Pending(QualityGateType.CodeReviewApproval, "Code review approval check not yet implemented"),
            QualityGateType.SecurityScan => QualityGateResult.Pending(QualityGateType.SecurityScan, "Security scan check not yet implemented"),
            QualityGateType.PerformanceTests => QualityGateResult.Pending(QualityGateType.PerformanceTests, "Performance tests check not yet implemented"),
            QualityGateType.DocumentationComplete => QualityGateResult.Pending(QualityGateType.DocumentationComplete, "Documentation completeness check not yet implemented"),
            _ => QualityGateResult.Pending(gateType, $"Quality gate type {gateType} not implemented")
        };
    }

    public async System.Threading.Tasks.Task<List<QualityGateResult>> EvaluateAllQualityGatesAsync(
        int releaseId,
        CancellationToken cancellationToken)
    {
        // Default quality gates to check
        var gateTypes = new[]
        {
            QualityGateType.AllTasksCompleted,
            QualityGateType.NoOpenBugs,
            QualityGateType.ManualApproval
        };

        var results = new List<QualityGateResult>();

        foreach (var gateType in gateTypes)
        {
            try
            {
                var result = await EvaluateQualityGateAsync(releaseId, gateType, cancellationToken);
                results.Add(result);

                _logger.LogInformation(
                    "Evaluated quality gate {GateType} for Release {ReleaseId}: {Status}",
                    gateType,
                    releaseId,
                    result.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error evaluating quality gate {GateType} for Release {ReleaseId}",
                    gateType,
                    releaseId);

                results.Add(QualityGateResult.Failed(
                    gateType,
                    $"Error evaluating quality gate: {ex.Message}"));
            }
        }

        return results;
    }

    private async System.Threading.Tasks.Task<QualityGateResult> CheckAllTasksCompleted(Release release)
    {
        var allTasks = release.Sprints
            .SelectMany(s => s.Items)
            .Select(si => si.UserStory)
            .Where(us => us != null)
            .SelectMany(us => us.Tasks)
            .ToList();

        if (!allTasks.Any())
        {
            return QualityGateResult.Warning(
                QualityGateType.AllTasksCompleted,
                "No tasks found in release sprints",
                null);
        }

        var completedTasks = allTasks.Count(t => t.Status == TaskConstants.Statuses.Done);
        var totalTasks = allTasks.Count;
        var completionRate = (decimal)completedTasks / totalTasks * 100;

        if (completedTasks == totalTasks)
        {
            return QualityGateResult.Success(
                QualityGateType.AllTasksCompleted,
                $"All {totalTasks} tasks completed",
                threshold: 100,
                actualValue: completionRate);
        }
        else
        {
            return QualityGateResult.Failed(
                QualityGateType.AllTasksCompleted,
                $"Only {completedTasks}/{totalTasks} tasks completed ({completionRate:F1}%)",
                $"Incomplete tasks: {totalTasks - completedTasks}",
                threshold: 100,
                actualValue: completionRate);
        }
    }

    private async System.Threading.Tasks.Task<QualityGateResult> CheckNoOpenBugs(Release release)
    {
        var allTasks = release.Sprints
            .SelectMany(s => s.Items)
            .Select(si => si.UserStory)
            .Where(us => us != null)
            .SelectMany(us => us.Tasks)
            .ToList();

        // Find open bugs (tasks with "bug" or "fix" in title that are not Done)
        var openBugs = allTasks
            .Where(t => (t.Title.ToLower().Contains("bug") || t.Title.ToLower().Contains("fix"))
                        && t.Status != TaskConstants.Statuses.Done)
            .ToList();

        if (!openBugs.Any())
        {
            return QualityGateResult.Success(
                QualityGateType.NoOpenBugs,
                "No open bugs found");
        }
        else
        {
            var bugDetails = string.Join(", ", openBugs.Take(5).Select(b => $"#{b.Id}: {b.Title}"));
            if (openBugs.Count > 5)
                bugDetails += $" (and {openBugs.Count - 5} more)";

            return QualityGateResult.Failed(
                QualityGateType.NoOpenBugs,
                $"{openBugs.Count} open bug(s) still need to be resolved",
                bugDetails);
        }
    }

    private QualityGateResult CheckManualApproval(Release release)
    {
        // Check if approval already exists in quality gates
        var existingGate = release.QualityGates
            .FirstOrDefault(qg => qg.Type == QualityGateType.ManualApproval);

        if (existingGate != null && existingGate.Status == QualityGateStatus.Passed)
        {
            var approverName = existingGate.CheckedByUser?.Username ?? "Unknown";
            return QualityGateResult.Success(
                QualityGateType.ManualApproval,
                $"Approved by {approverName}");
        }

        return QualityGateResult.Pending(
            QualityGateType.ManualApproval,
            "Awaiting manual approval from stakeholder");
    }
}

