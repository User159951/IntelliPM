using FluentValidation;

namespace IntelliPM.Application.Projects.Commands;

/// <summary>
/// Validator for RemoveMemberCommand.
/// </summary>
public class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0)
            .WithMessage("Current user ID must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x)
            .Must(x => x.CurrentUserId != x.UserId)
            .WithMessage("Cannot remove yourself from the project")
            .When(x => x.CurrentUserId > 0 && x.UserId > 0);
    }
}

