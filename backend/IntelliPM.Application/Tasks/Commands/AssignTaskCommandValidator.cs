using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for AssignTaskCommand.
/// </summary>
public class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
    public AssignTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0");

        RuleFor(x => x.UpdatedBy)
            .GreaterThan(0)
            .WithMessage("UpdatedBy user ID must be greater than 0");

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0)
            .WithMessage("Assignee ID must be greater than 0 if provided")
            .When(x => x.AssigneeId.HasValue);
    }
}

