using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for AddTaskDependencyCommand.
/// </summary>
public class AddTaskDependencyCommandValidator : AbstractValidator<AddTaskDependencyCommand>
{
    public AddTaskDependencyCommandValidator()
    {
        RuleFor(x => x.SourceTaskId)
            .GreaterThan(0)
            .WithMessage("Source task ID must be greater than 0");

        RuleFor(x => x.DependentTaskId)
            .GreaterThan(0)
            .WithMessage("Dependent task ID must be greater than 0");

        RuleFor(x => x.DependentTaskId)
            .NotEqual(x => x.SourceTaskId)
            .WithMessage("A task cannot depend on itself");
    }
}

