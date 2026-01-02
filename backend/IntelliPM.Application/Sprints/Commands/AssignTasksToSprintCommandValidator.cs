using FluentValidation;

namespace IntelliPM.Application.Sprints.Commands;

/// <summary>
/// Validator for AssignTasksToSprintCommand.
/// </summary>
public class AssignTasksToSprintCommandValidator : AbstractValidator<AssignTasksToSprintCommand>
{
    public AssignTasksToSprintCommandValidator()
    {
        RuleFor(x => x.SprintId)
            .GreaterThan(0)
            .WithMessage("Sprint ID must be greater than 0");

        RuleFor(x => x.TaskIds)
            .NotNull()
            .WithMessage("Task IDs list cannot be null")
            .NotEmpty()
            .WithMessage("At least one task ID is required");

        RuleForEach(x => x.TaskIds)
            .GreaterThan(0)
            .WithMessage("Each task ID must be greater than 0");

        RuleFor(x => x.UpdatedBy)
            .GreaterThan(0)
            .WithMessage("UpdatedBy user ID must be greater than 0");
    }
}

