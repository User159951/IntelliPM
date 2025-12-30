using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for RemoveTaskDependencyCommand.
/// </summary>
public class RemoveTaskDependencyCommandValidator : AbstractValidator<RemoveTaskDependencyCommand>
{
    public RemoveTaskDependencyCommandValidator()
    {
        RuleFor(x => x.DependencyId)
            .GreaterThan(0)
            .WithMessage("Dependency ID must be greater than 0");
    }
}

