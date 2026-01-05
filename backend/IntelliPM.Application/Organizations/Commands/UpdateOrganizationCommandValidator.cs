using FluentValidation;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Validator for UpdateOrganizationCommand.
/// </summary>
public class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");

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

