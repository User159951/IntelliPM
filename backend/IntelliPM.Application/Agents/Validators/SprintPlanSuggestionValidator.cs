using FluentValidation;
using IntelliPM.Application.Agents.DTOs;

namespace IntelliPM.Application.Agents.Validators;

/// <summary>
/// Validator for SprintPlanSuggestionDto.
/// </summary>
public class SprintPlanSuggestionValidator : AbstractValidator<SprintPlanSuggestionDto>
{
    public SprintPlanSuggestionValidator()
    {
        RuleFor(x => x.SuggestedTasks)
            .NotNull()
            .WithMessage("Suggested tasks list cannot be null");

        RuleFor(x => x.TotalStoryPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total story points must be non-negative");

        RuleFor(x => x.CapacityUtilization)
            .InclusiveBetween(0.0, 1.5)
            .WithMessage("Capacity utilization must be between 0.0 and 1.5 (150%)");

        RuleForEach(x => x.SuggestedTasks)
            .SetValidator(new SprintTaskSuggestionValidator());
    }
}

/// <summary>
/// Validator for SprintTaskSuggestion.
/// </summary>
public class SprintTaskSuggestionValidator : AbstractValidator<SprintTaskSuggestion>
{
    public SprintTaskSuggestionValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0");

        RuleFor(x => x.TaskTitle)
            .NotEmpty()
            .WithMessage("Task title is required")
            .MaximumLength(500)
            .WithMessage("Task title must not exceed 500 characters");

        RuleFor(x => x.StoryPoints)
            .GreaterThanOrEqualTo(0)
            .When(x => x.StoryPoints.HasValue)
            .WithMessage("Story points must be non-negative");

        RuleFor(x => x.AssignedTo)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.AssignedTo))
            .WithMessage("AssignedTo must not exceed 200 characters");
    }
}

