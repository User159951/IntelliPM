using FluentValidation;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Validator for DisableAIForOrgCommand.
/// Ensures organization ID and reason are provided.
/// </summary>
public class DisableAIForOrgCommandValidator : AbstractValidator<DisableAIForOrgCommand>
{
    public DisableAIForOrgCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters");
    }
}

