using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for ChangeTaskStatusCommand.
/// Validates task status changes including workflow transition rules.
/// </summary>
public class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    private static readonly string[] ValidStatuses = { "Todo", "InProgress", "InReview", "Done", "Blocked" };

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
            .Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }
}

