using FluentValidation;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Validator for UpdateMemberAIQuotaCommand.
/// </summary>
public class UpdateMemberAIQuotaCommandValidator : AbstractValidator<UpdateMemberAIQuotaCommand>
{
    public UpdateMemberAIQuotaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.MonthlyTokenLimitOverride)
            .GreaterThan(0)
            .WithMessage("Monthly token limit override must be greater than 0 if provided")
            .LessThanOrEqualTo(1_000_000_000L) // 1 billion tokens max
            .WithMessage("Monthly token limit override cannot exceed 1,000,000,000")
            .When(x => x.MonthlyTokenLimitOverride.HasValue);

        RuleFor(x => x.MonthlyRequestLimitOverride)
            .GreaterThan(0)
            .WithMessage("Monthly request limit override must be greater than 0 if provided")
            .LessThanOrEqualTo(10_000_000) // 10 million requests max
            .WithMessage("Monthly request limit override cannot exceed 10,000,000")
            .When(x => x.MonthlyRequestLimitOverride.HasValue);
    }
}

