using FluentValidation;

namespace IntelliPM.Application.Sprints.Commands;

/// <summary>
/// Validator for CompleteSprintCommand.
/// </summary>
public class CompleteSprintCommandValidator : AbstractValidator<CompleteSprintCommand>
{
    public CompleteSprintCommandValidator()
    {
        RuleFor(x => x.SprintId)
            .GreaterThan(0)
            .WithMessage("Sprint ID must be greater than 0");

        RuleFor(x => x.UpdatedBy)
            .GreaterThan(0)
            .WithMessage("UpdatedBy user ID must be greater than 0");

        RuleFor(x => x.IncompleteTasksAction)
            .Must(action => action == null || 
                action == "next_sprint" || 
                action == "backlog" || 
                action == "keep")
            .WithMessage("IncompleteTasksAction must be one of: next_sprint, backlog, keep")
            .When(x => !string.IsNullOrWhiteSpace(x.IncompleteTasksAction));
    }
}

