using FluentValidation;

namespace IntelliPM.Application.FeatureFlags.Commands;

/// <summary>
/// Validator for CreateFeatureFlagCommand.
/// </summary>
public class CreateFeatureFlagCommandValidator : AbstractValidator<CreateFeatureFlagCommand>
{
    public CreateFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Feature flag name is required.")
            .MaximumLength(100).WithMessage("Feature flag name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Feature flag name must contain only letters, numbers, and underscores.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0).WithMessage("Organization ID must be greater than 0.")
            .When(x => x.OrganizationId.HasValue);
    }
}

