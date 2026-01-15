using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for UpdateTaskCommand.
/// Validates task updates with comprehensive business rules.
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    private static readonly string[] ValidPriorities = { "Low", "Medium", "High", "Critical" };

    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0");

        RuleFor(x => x.Title)
            .MinimumLength(1)
            .WithMessage("Task title cannot be empty")
            .MaximumLength(500)
            .WithMessage("Task title cannot exceed 500 characters")
            .Must(title => string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(title.Trim()))
            .WithMessage("Task title cannot be whitespace only")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Description)
            .MinimumLength(1)
            .WithMessage("Task description cannot be empty")
            .MaximumLength(10000)
            .WithMessage("Task description cannot exceed 10000 characters")
            .Must(desc => string.IsNullOrWhiteSpace(desc) || !string.IsNullOrWhiteSpace(desc.Trim()))
            .WithMessage("Task description cannot be whitespace only")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Priority)
            .Must(priority => priority == null || ValidPriorities.Contains(priority))
            .WithMessage($"Priority must be one of: {string.Join(", ", ValidPriorities)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));

        RuleFor(x => x.StoryPoints)
            .InclusiveBetween(1, 100)
            .WithMessage("Story points must be between 1 and 100")
            .When(x => x.StoryPoints.HasValue);

        RuleFor(x => x.UpdatedById)
            .GreaterThan(0)
            .WithMessage("UpdatedBy user ID must be greater than 0");
    }
}

