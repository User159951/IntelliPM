using FluentValidation;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Validator for UpdateAIQuotaCommand.
/// Ensures all quota values are valid and within acceptable ranges.
/// </summary>
public class UpdateAIQuotaCommandValidator : AbstractValidator<UpdateAIQuotaCommand>
{
    public UpdateAIQuotaCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");

        RuleFor(x => x.TierName)
            .NotEmpty()
            .WithMessage("Tier name is required")
            .Must(tier => new[] { AIQuotaConstants.Tiers.Free, AIQuotaConstants.Tiers.Pro, AIQuotaConstants.Tiers.Enterprise, AIQuotaConstants.Tiers.Custom }.Contains(tier))
            .WithMessage($"Invalid tier name. Must be: {AIQuotaConstants.Tiers.Free}, {AIQuotaConstants.Tiers.Pro}, {AIQuotaConstants.Tiers.Enterprise}, or {AIQuotaConstants.Tiers.Custom}");

        RuleFor(x => x.MaxTokensPerPeriod)
            .GreaterThan(0)
            .When(x => x.MaxTokensPerPeriod.HasValue)
            .WithMessage("Max tokens per period must be greater than 0");

        RuleFor(x => x.MaxRequestsPerPeriod)
            .GreaterThan(0)
            .When(x => x.MaxRequestsPerPeriod.HasValue)
            .WithMessage("Max requests per period must be greater than 0");

        RuleFor(x => x.MaxDecisionsPerPeriod)
            .GreaterThan(0)
            .When(x => x.MaxDecisionsPerPeriod.HasValue)
            .WithMessage("Max decisions per period must be greater than 0");

        RuleFor(x => x.MaxCostPerPeriod)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxCostPerPeriod.HasValue)
            .WithMessage("Max cost per period must be greater than or equal to 0");

        RuleFor(x => x.OverageRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.OverageRate.HasValue)
            .WithMessage("Overage rate must be greater than or equal to 0");

        RuleFor(x => x.ScheduledDate)
            .GreaterThan(DateTimeOffset.UtcNow)
            .When(x => !x.ApplyImmediately && x.ScheduledDate.HasValue)
            .WithMessage("Scheduled date must be in the future");
    }
}

