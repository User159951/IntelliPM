using FluentValidation;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Validator for CreateOrganizationCommand.
/// </summary>
public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Organization name is required")
            .MaximumLength(200)
            .WithMessage("Organization name cannot exceed 200 characters");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Organization code is required")
            .MaximumLength(100)
            .WithMessage("Organization code cannot exceed 100 characters")
            .Matches(@"^[a-z0-9-]+$")
            .WithMessage("Organization code must contain only lowercase letters, numbers, and hyphens");
    }
}

