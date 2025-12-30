using FluentValidation;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Projects.Commands;

public class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("NewRole must be a valid ProjectRole value");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0).WithMessage("Current user ID must be greater than 0");
    }
}

