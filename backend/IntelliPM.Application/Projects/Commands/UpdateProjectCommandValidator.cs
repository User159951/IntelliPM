using FluentValidation;

namespace IntelliPM.Application.Projects.Commands;

/// <summary>
/// Validator for UpdateProjectCommand.
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0)
            .WithMessage("Current user ID must be greater than 0");

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage("Project name cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .WithMessage("Project description cannot exceed 5000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.SprintDurationDays)
            .InclusiveBetween(1, 30)
            .WithMessage("Sprint duration must be between 1 and 30 days")
            .When(x => x.SprintDurationDays.HasValue);

        RuleFor(x => x.Status)
            .Must(status => status == null || 
                status == "Active" || 
                status == "OnHold" || 
                status == "Archived" || 
                status == "Completed")
            .WithMessage("Status must be one of: Active, OnHold, Archived, Completed")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.Type)
            .Must(type => type == null || 
                type == "Software" || 
                type == "Hardware" || 
                type == "Research" || 
                type == "Other")
            .WithMessage("Type must be one of: Software, Hardware, Research, Other")
            .When(x => !string.IsNullOrWhiteSpace(x.Type));
    }
}

