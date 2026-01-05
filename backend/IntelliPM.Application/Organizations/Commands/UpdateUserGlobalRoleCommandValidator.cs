using FluentValidation;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Validator for UpdateUserGlobalRoleCommand.
/// </summary>
public class UpdateUserGlobalRoleCommandValidator : AbstractValidator<UpdateUserGlobalRoleCommand>
{
    public UpdateUserGlobalRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.GlobalRole)
            .IsInEnum()
            .WithMessage("Global role must be a valid role value");
    }
}

