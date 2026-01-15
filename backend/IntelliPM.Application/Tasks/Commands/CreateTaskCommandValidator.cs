using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for CreateTaskCommand.
/// Validates task creation with comprehensive business rules.
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    private static readonly string[] ValidPriorities = { "Low", "Medium", "High", "Critical" };

    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Task title is required")
            .MinimumLength(1)
            .WithMessage("Task title cannot be empty")
            .MaximumLength(500)
            .WithMessage("Task title cannot exceed 500 characters")
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Task title cannot be whitespace only");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Task description is required")
            .MinimumLength(1)
            .WithMessage("Task description cannot be empty")
            .MaximumLength(10000)
            .WithMessage("Task description cannot exceed 10000 characters")
            .Must(desc => !string.IsNullOrWhiteSpace(desc))
            .WithMessage("Task description cannot be whitespace only");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .WithMessage("Priority is required")
            .Must(priority => ValidPriorities.Contains(priority))
            .WithMessage($"Priority must be one of: {string.Join(", ", ValidPriorities)}");

        RuleFor(x => x.StoryPoints)
            .InclusiveBetween(1, 100)
            .WithMessage("Story points must be between 1 and 100")
            .When(x => x.StoryPoints.HasValue);

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0)
            .WithMessage("Assignee ID must be greater than 0 if provided")
            .When(x => x.AssigneeId.HasValue);

        RuleFor(x => x.CreatedById)
            .GreaterThan(0)
            .WithMessage("CreatedBy user ID must be greater than 0");
    }
}

