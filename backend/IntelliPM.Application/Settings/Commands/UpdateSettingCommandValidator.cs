using FluentValidation;

namespace IntelliPM.Application.Settings.Commands;

public class UpdateSettingCommandValidator : AbstractValidator<UpdateSettingCommand>
{
    public UpdateSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting key is required.")
            .MaximumLength(100).WithMessage("Setting key must not exceed 100 characters.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Setting value is required.")
            .MaximumLength(1000).WithMessage("Setting value must not exceed 1000 characters.");
    }
}

