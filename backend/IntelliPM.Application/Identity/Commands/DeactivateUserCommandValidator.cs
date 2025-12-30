using FluentValidation;

namespace IntelliPM.Application.Identity.Commands;

/// <summary>
/// Validator for DeactivateUserCommand.
/// </summary>
public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");
    }
}

