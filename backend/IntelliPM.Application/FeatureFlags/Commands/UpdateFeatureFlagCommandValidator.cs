using FluentValidation;

namespace IntelliPM.Application.FeatureFlags.Commands;

/// <summary>
/// Validator for UpdateFeatureFlagCommand.
/// </summary>
public class UpdateFeatureFlagCommandValidator : AbstractValidator<UpdateFeatureFlagCommand>
{
    public UpdateFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Feature flag ID is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(x => x.IsEnabled.HasValue || !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("At least one field (IsEnabled or Description) must be provided for update.");
    }
}

