using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for CreateTaskCommand.
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Task title is required")
            .MaximumLength(500)
            .WithMessage("Task title cannot exceed 500 characters");

        RuleFor(x => x.Description)
            .MaximumLength(10000)
            .WithMessage("Task description cannot exceed 10000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .WithMessage("Priority is required")
            .Must(priority => priority == "Low" || 
                priority == "Medium" || 
                priority == "High" || 
                priority == "Critical")
            .WithMessage("Priority must be one of: Low, Medium, High, Critical");

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

