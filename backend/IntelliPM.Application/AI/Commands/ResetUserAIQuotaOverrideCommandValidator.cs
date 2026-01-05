using FluentValidation;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Validator for ResetUserAIQuotaOverrideCommand.
/// </summary>
public class ResetUserAIQuotaOverrideCommandValidator : AbstractValidator<ResetUserAIQuotaOverrideCommand>
{
    public ResetUserAIQuotaOverrideCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");
    }
}

