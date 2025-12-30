using FluentValidation;

namespace IntelliPM.Application.Identity.Commands;

/// <summary>
/// Validator for ActivateUserCommand.
/// </summary>
public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");
    }
}

