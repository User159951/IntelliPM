using FluentValidation;

namespace IntelliPM.Application.Projects.Commands;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MinimumLength(3).WithMessage("Project name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Project type is required")
            .Must(type => type == "Scrum" || type == "Kanban")
            .WithMessage("Project type must be either 'Scrum' or 'Kanban'");

        RuleFor(x => x.SprintDurationDays)
            .GreaterThan(0).WithMessage("Sprint duration must be greater than 0")
            .LessThanOrEqualTo(30).WithMessage("Sprint duration must not exceed 30 days");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0).WithMessage("Valid owner ID is required");
    }
}

