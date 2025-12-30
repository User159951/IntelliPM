using IntelliPM.Application.Common;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for validating milestone business rules and constraints.
/// Implements cross-entity validations that require database access.
/// </summary>
public class MilestoneValidator : IMilestoneValidator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MilestoneValidator> _logger;

    public MilestoneValidator(
        IUnitOfWork unitOfWork,
        ILogger<MilestoneValidator> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates that the due date is appropriate for the project.
    /// Checks that the due date is in the future and within project constraints.
    /// </summary>
    public async Task<ValidationResult> ValidateDueDateAsync(
        int projectId,
        DateTimeOffset dueDate,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Get project to check constraints
        var project = await _unitOfWork.Repository<Project>()
            .GetByIdAsync(projectId, cancellationToken);

        if (project == null)
        {
            var error = $"Project with ID {projectId} not found.";
            _logger.LogWarning("Validation failed: {Error}", error);
            return ValidationResult.Failure(error);
        }

        // Check due date is in the future (when creating)
        var now = DateTimeOffset.UtcNow;
        if (dueDate <= now)
        {
            var error = MilestoneConstants.Validation.ErrorDueDateInPast;
            errors.Add(error);
            _logger.LogWarning(
                "Validation failed: Due date {DueDate} is not in the future for milestone in project {ProjectId}",
                dueDate,
                projectId);
        }

        // Note: Project entity doesn't have StartDate/EndDate properties currently
        // If these are added in the future, uncomment the following validations:
        /*
        // Check due date is after project start date (if project has start date)
        if (project.StartDate.HasValue && dueDate < project.StartDate.Value)
        {
            var error = MilestoneConstants.Validation.ErrorDueDateBeforeProjectStart;
            errors.Add(error);
            _logger.LogWarning(
                "Validation failed: Due date {DueDate} is before project start date {StartDate} for project {ProjectId}",
                dueDate,
                project.StartDate.Value,
                projectId);
        }

        // Check due date is before project end date (if project has end date)
        if (project.EndDate.HasValue && dueDate > project.EndDate.Value)
        {
            var error = MilestoneConstants.Validation.ErrorDueDateAfterProjectEnd;
            errors.Add(error);
            _logger.LogWarning(
                "Validation failed: Due date {DueDate} is after project end date {EndDate} for project {ProjectId}",
                dueDate,
                project.EndDate.Value,
                projectId);
        }
        */

        if (errors.Count > 0)
        {
            return ValidationResult.Failure(errors.ToArray());
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates milestone status transition.
    /// Ensures that the transition from current status to new status is allowed.
    /// </summary>
    public ValidationResult ValidateStatusTransition(
        MilestoneStatus currentStatus,
        MilestoneStatus newStatus)
    {
        // No transition needed if status is the same
        if (currentStatus == newStatus)
        {
            return ValidationResult.Success();
        }

        // Define valid transitions
        var validTransitions = new Dictionary<MilestoneStatus, List<MilestoneStatus>>
        {
            { MilestoneStatus.Pending, new List<MilestoneStatus> { MilestoneStatus.InProgress, MilestoneStatus.Cancelled } },
            { MilestoneStatus.InProgress, new List<MilestoneStatus> { MilestoneStatus.Completed, MilestoneStatus.Missed, MilestoneStatus.Cancelled } },
            { MilestoneStatus.Completed, new List<MilestoneStatus>() }, // Cannot transition from Completed
            { MilestoneStatus.Missed, new List<MilestoneStatus>() }, // Cannot transition from Missed
            { MilestoneStatus.Cancelled, new List<MilestoneStatus>() } // Cannot transition from Cancelled
        };

        // Check if transition is valid
        if (validTransitions.TryGetValue(currentStatus, out var allowedStatuses))
        {
            if (allowedStatuses.Contains(newStatus))
            {
                return ValidationResult.Success();
            }
        }

        // Invalid transition
        var error = string.Format(
            MilestoneConstants.Validation.ErrorInvalidStatusTransition,
            currentStatus,
            newStatus);

        _logger.LogWarning(
            "Invalid status transition: {CurrentStatus} -> {NewStatus}",
            currentStatus,
            newStatus);

        return ValidationResult.Failure(error);
    }

    /// <summary>
    /// Validates milestone completion.
    /// Ensures that completion date and status are consistent.
    /// </summary>
    public ValidationResult ValidateCompletion(
        Milestone milestone,
        DateTimeOffset? completedAt)
    {
        var errors = new List<string>();
        var now = DateTimeOffset.UtcNow;

        // If completedAt is set, validate it
        if (completedAt.HasValue)
        {
            // Cannot be in the future
            if (completedAt.Value > now)
            {
                var error = MilestoneConstants.Validation.ErrorCompletedAtInFuture;
                errors.Add(error);
                _logger.LogWarning(
                    "Validation failed: Completion date {CompletedAt} is in the future for milestone {MilestoneId}",
                    completedAt.Value,
                    milestone.Id);
            }

            // Cannot be before creation date
            if (completedAt.Value < milestone.CreatedAt)
            {
                var error = MilestoneConstants.Validation.ErrorCompletedAtBeforeCreated;
                errors.Add(error);
                _logger.LogWarning(
                    "Validation failed: Completion date {CompletedAt} is before creation date {CreatedAt} for milestone {MilestoneId}",
                    completedAt.Value,
                    milestone.CreatedAt,
                    milestone.Id);
            }
        }

        // If status is Completed, completedAt must be set
        if (milestone.Status == MilestoneStatus.Completed && !completedAt.HasValue)
        {
            var error = MilestoneConstants.Validation.ErrorCompletedWithoutCompletedAt;
            errors.Add(error);
            _logger.LogWarning(
                "Validation failed: Milestone {MilestoneId} is marked as Completed but has no completion date",
                milestone.Id);
        }

        // If status is Pending or InProgress, completedAt should be null
        if ((milestone.Status == MilestoneStatus.Pending || milestone.Status == MilestoneStatus.InProgress) 
            && completedAt.HasValue)
        {
            var error = $"Milestone with status {milestone.Status} should not have a completion date.";
            errors.Add(error);
            _logger.LogWarning(
                "Validation failed: Milestone {MilestoneId} has status {Status} but has completion date {CompletedAt}",
                milestone.Id,
                milestone.Status,
                completedAt.Value);
        }

        // Warning: Progress should be 100 when completed (not an error, just a warning)
        if (milestone.Status == MilestoneStatus.Completed && milestone.Progress < 100)
        {
            _logger.LogWarning(
                "Milestone {MilestoneId} is marked as Completed but progress is {Progress}% (expected 100%)",
                milestone.Id,
                milestone.Progress);
        }

        if (errors.Count > 0)
        {
            return ValidationResult.Failure(errors.ToArray());
        }

        return ValidationResult.Success();
    }
}

