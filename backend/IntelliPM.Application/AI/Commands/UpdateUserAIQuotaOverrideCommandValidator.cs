using FluentValidation;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Validator for UpdateUserAIQuotaOverrideCommand.
/// </summary>
public class UpdateUserAIQuotaOverrideCommandValidator : AbstractValidator<UpdateUserAIQuotaOverrideCommand>
{
    public UpdateUserAIQuotaOverrideCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.MaxTokensPerPeriod)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxTokensPerPeriod.HasValue)
            .WithMessage("Max tokens must be >= 0")
            .LessThanOrEqualTo(10_000_000)
            .When(x => x.MaxTokensPerPeriod.HasValue)
            .WithMessage("Max tokens cannot exceed 10,000,000");

        RuleFor(x => x.MaxRequestsPerPeriod)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxRequestsPerPeriod.HasValue)
            .WithMessage("Max requests must be >= 0")
            .LessThanOrEqualTo(100_000)
            .When(x => x.MaxRequestsPerPeriod.HasValue)
            .WithMessage("Max requests cannot exceed 100,000");

        RuleFor(x => x.MaxDecisionsPerPeriod)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxDecisionsPerPeriod.HasValue)
            .WithMessage("Max decisions must be >= 0")
            .LessThanOrEqualTo(50_000)
            .When(x => x.MaxDecisionsPerPeriod.HasValue)
            .WithMessage("Max decisions cannot exceed 50,000");

        RuleFor(x => x.MaxCostPerPeriod)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxCostPerPeriod.HasValue)
            .WithMessage("Max cost must be >= 0")
            .LessThanOrEqualTo(10_000)
            .When(x => x.MaxCostPerPeriod.HasValue)
            .WithMessage("Max cost cannot exceed $10,000");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Reason cannot exceed 500 characters");
    }
}

