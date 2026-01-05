using FluentValidation;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Validator for UpsertOrganizationAIQuotaCommand.
/// </summary>
public class UpsertOrganizationAIQuotaCommandValidator : AbstractValidator<UpsertOrganizationAIQuotaCommand>
{
    public UpsertOrganizationAIQuotaCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");

        RuleFor(x => x.MonthlyTokenLimit)
            .GreaterThan(0)
            .WithMessage("Monthly token limit must be greater than 0")
            .LessThanOrEqualTo(1_000_000_000L) // 1 billion tokens max
            .WithMessage("Monthly token limit cannot exceed 1,000,000,000");

        RuleFor(x => x.MonthlyRequestLimit)
            .GreaterThan(0)
            .WithMessage("Monthly request limit must be greater than 0 if provided")
            .LessThanOrEqualTo(10_000_000) // 10 million requests max
            .WithMessage("Monthly request limit cannot exceed 10,000,000")
            .When(x => x.MonthlyRequestLimit.HasValue);

        RuleFor(x => x.ResetDayOfMonth)
            .InclusiveBetween(1, 31)
            .WithMessage("Reset day of month must be between 1 and 31")
            .When(x => x.ResetDayOfMonth.HasValue);
    }
}

