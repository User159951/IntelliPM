using FluentValidation;

namespace IntelliPM.Application.Sprints.Commands;

/// <summary>
/// Validator for AddTaskToSprintCommand.
/// </summary>
public class AddTaskToSprintCommandValidator : AbstractValidator<AddTaskToSprintCommand>
{
    public AddTaskToSprintCommandValidator()
    {
        RuleFor(x => x.SprintId)
            .GreaterThan(0)
            .WithMessage("Sprint ID must be greater than 0");

        RuleFor(x => x.TaskIds)
            .NotEmpty()
            .WithMessage("At least one task ID is required");

        RuleFor(x => x.TaskIds)
            .Must(taskIds => taskIds.Distinct().Count() == taskIds.Count)
            .WithMessage("Task IDs must be unique (no duplicates)");

        RuleForEach(x => x.TaskIds)
            .GreaterThan(0)
            .WithMessage("Each task ID must be greater than 0");
    }
}

