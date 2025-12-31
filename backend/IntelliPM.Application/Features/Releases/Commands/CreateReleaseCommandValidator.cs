using FluentValidation;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Validator for CreateReleaseCommand.
/// </summary>
public class CreateReleaseCommandValidator : AbstractValidator<CreateReleaseCommand>
{
    public CreateReleaseCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Release name is required")
            .MaximumLength(200)
            .WithMessage("Release name cannot exceed 200 characters");

        RuleFor(x => x.Version)
            .NotEmpty()
            .WithMessage("Version is required")
            .MaximumLength(50)
            .WithMessage("Version cannot exceed 50 characters")
            .Matches(@"^(\d+)\.(\d+)(\.(\d+))?(-[a-zA-Z0-9]+)?$")
            .WithMessage("Version must follow semantic versioning format (e.g., 1.0.0, 2.1.0-beta)");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PlannedDate)
            .NotEmpty()
            .WithMessage("Planned date is required")
            .GreaterThan(DateTimeOffset.UtcNow.AddDays(-1))
            .WithMessage("Planned date should be in the future or very recent past");

        RuleFor(x => x.TagName)
            .MaximumLength(100)
            .WithMessage("Tag name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.TagName));
    }
}
