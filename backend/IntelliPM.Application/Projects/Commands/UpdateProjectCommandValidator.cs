using FluentValidation;

namespace IntelliPM.Application.Projects.Commands;

/// <summary>
/// Validator for UpdateProjectCommand.
/// Validates project updates with comprehensive business rules.
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    private static readonly string[] ValidStatuses = { "Active", "OnHold", "Archived", "Completed" };
    private static readonly string[] ValidTypes = { "Software", "Hardware", "Research", "Other", "Scrum", "Kanban" };

    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0)
            .WithMessage("Current user ID must be greater than 0");

        RuleFor(x => x.Name)
            .MinimumLength(3)
            .WithMessage("Project name must be at least 3 characters")
            .MaximumLength(200)
            .WithMessage("Project name cannot exceed 200 characters")
            .Must(name => string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(name.Trim()))
            .WithMessage("Project name cannot be whitespace only")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .WithMessage("Project description cannot exceed 5000 characters")
            .Must(desc => string.IsNullOrWhiteSpace(desc) || !string.IsNullOrWhiteSpace(desc.Trim()))
            .WithMessage("Project description cannot be whitespace only")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.SprintDurationDays)
            .GreaterThan(0)
            .WithMessage("Sprint duration must be greater than 0")
            .LessThanOrEqualTo(30)
            .WithMessage("Sprint duration must not exceed 30 days")
            .When(x => x.SprintDurationDays.HasValue);

        RuleFor(x => x.Status)
            .Must(status => status == null || ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.Type)
            .Must(type => type == null || ValidTypes.Contains(type))
            .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Type));
    }
}

