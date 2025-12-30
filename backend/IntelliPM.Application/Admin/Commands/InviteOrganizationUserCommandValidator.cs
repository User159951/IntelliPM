using FluentValidation;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Admin.Commands;

public class InviteOrganizationUserCommandValidator : AbstractValidator<InviteOrganizationUserCommand>
{
    public InviteOrganizationUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be Admin or User")
            .NotEqual(default(GlobalRole)).WithMessage("Role is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
    }
}

