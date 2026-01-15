using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for AssignTaskCommand.
/// Validates task assignment with comprehensive business rules.
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

        // Ensure assignee is not the same as the updater (optional business rule)
        // This can be removed if self-assignment is allowed
        RuleFor(x => x)
            .Must(cmd => !cmd.AssigneeId.HasValue || cmd.AssigneeId.Value != cmd.UpdatedBy)
            .WithMessage("Cannot assign task to the same user who is updating it")
            .When(x => x.AssigneeId.HasValue);
    }
}

