using FluentValidation;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Projects.Commands;

/// <summary>
/// Validator for AssignTeamToProjectCommand.
/// </summary>
public class AssignTeamToProjectCommandValidator : AbstractValidator<AssignTeamToProjectCommand>
{
    public AssignTeamToProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Valid project ID is required");

        RuleFor(x => x.TeamId)
            .GreaterThan(0)
            .WithMessage("Valid team ID is required");

        RuleFor(x => x.DefaultRole)
            .IsInEnum()
            .WithMessage("Default role must be a valid ProjectRole value");

        RuleFor(x => x.MemberRoleOverrides)
            .Must(overrides => overrides == null || overrides.All(kvp => kvp.Key > 0 && Enum.IsDefined(typeof(ProjectRole), kvp.Value)))
            .WithMessage("Member role overrides must have valid user IDs (> 0) and valid ProjectRole values")
            .When(x => x.MemberRoleOverrides != null);
    }
}

