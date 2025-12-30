using IntelliPM.Application.Common;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service interface for validating milestone business rules and constraints.
/// Handles cross-entity validations that require database access.
/// </summary>
public interface IMilestoneValidator
{
    /// <summary>
    /// Validates that the due date is appropriate for the project.
    /// Checks that the due date is in the future and within project constraints.
    /// </summary>
    /// <param name="projectId">The ID of the project the milestone belongs to.</param>
    /// <param name="dueDate">The due date to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result indicating whether the due date is valid.</returns>
    Task<ValidationResult> ValidateDueDateAsync(int projectId, DateTimeOffset dueDate, CancellationToken cancellationToken);

    /// <summary>
    /// Validates milestone status transition.
    /// Ensures that the transition from current status to new status is allowed.
    /// </summary>
    /// <param name="currentStatus">The current status of the milestone.</param>
    /// <param name="newStatus">The new status to transition to.</param>
    /// <returns>Validation result indicating whether the transition is valid.</returns>
    ValidationResult ValidateStatusTransition(MilestoneStatus currentStatus, MilestoneStatus newStatus);

    /// <summary>
    /// Validates milestone completion.
    /// Ensures that completion date and status are consistent.
    /// </summary>
    /// <param name="milestone">The milestone to validate.</param>
    /// <param name="completedAt">The completion date to validate.</param>
    /// <returns>Validation result indicating whether the completion is valid.</returns>
    ValidationResult ValidateCompletion(Milestone milestone, DateTimeOffset? completedAt);
}

