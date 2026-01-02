using FluentValidation;

namespace IntelliPM.Application.Sprints.Commands;

/// <summary>
/// Validator for CreateSprintCommand.
/// </summary>
public class CreateSprintCommandValidator : AbstractValidator<CreateSprintCommand>
{
    public CreateSprintCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Sprint name is required")
            .MaximumLength(200)
            .WithMessage("Sprint name cannot exceed 200 characters");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0)
            .WithMessage("Current user ID must be greater than 0");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than 0");

        RuleFor(x => x.Goal)
            .MaximumLength(1000)
            .WithMessage("Goal cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Goal));
    }
}

