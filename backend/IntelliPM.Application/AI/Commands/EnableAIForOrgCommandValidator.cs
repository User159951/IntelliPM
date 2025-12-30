using FluentValidation;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Validator for EnableAIForOrgCommand.
/// Ensures organization ID and valid tier name are provided.
/// </summary>
public class EnableAIForOrgCommandValidator : AbstractValidator<EnableAIForOrgCommand>
{
    public EnableAIForOrgCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");

        RuleFor(x => x.TierName)
            .NotEmpty()
            .WithMessage("Tier name is required")
            .Must(tier => new[] { AIQuotaConstants.Tiers.Free, AIQuotaConstants.Tiers.Pro, AIQuotaConstants.Tiers.Enterprise }.Contains(tier))
            .WithMessage($"Invalid tier name. Must be: {AIQuotaConstants.Tiers.Free}, {AIQuotaConstants.Tiers.Pro}, or {AIQuotaConstants.Tiers.Enterprise}");
    }
}

