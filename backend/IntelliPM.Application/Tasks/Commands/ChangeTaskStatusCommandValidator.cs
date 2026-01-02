using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for ChangeTaskStatusCommand.
/// </summary>
public class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0");

        RuleFor(x => x.UpdatedBy)
            .GreaterThan(0)
            .WithMessage("UpdatedBy user ID must be greater than 0");

        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .WithMessage("New status is required")
            .Must(status => status == "Todo" || 
                status == "InProgress" || 
                status == "Blocked" || 
                status == "Done")
            .WithMessage("Status must be one of: Todo, InProgress, Blocked, Done");
    }
}

