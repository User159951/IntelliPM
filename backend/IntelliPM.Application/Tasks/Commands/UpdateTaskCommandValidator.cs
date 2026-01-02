using FluentValidation;

namespace IntelliPM.Application.Tasks.Commands;

/// <summary>
/// Validator for UpdateTaskCommand.
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0");

        RuleFor(x => x.Title)
            .MaximumLength(500)
            .WithMessage("Task title cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(10000)
            .WithMessage("Task description cannot exceed 10000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Priority)
            .Must(priority => priority == null || 
                priority == "Low" || 
                priority == "Medium" || 
                priority == "High" || 
                priority == "Critical")
            .WithMessage("Priority must be one of: Low, Medium, High, Critical")
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

