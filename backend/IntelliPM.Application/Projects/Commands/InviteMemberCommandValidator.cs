using FluentValidation;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Projects.Commands;

public class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be in a valid format");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid ProjectRole value");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0).WithMessage("Current user ID must be greater than 0");
    }
}

