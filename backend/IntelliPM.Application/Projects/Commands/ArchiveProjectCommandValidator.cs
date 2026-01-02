using FluentValidation;

namespace IntelliPM.Application.Projects.Commands;

/// <summary>
/// Validator for ArchiveProjectCommand.
/// </summary>
public class ArchiveProjectCommandValidator : AbstractValidator<ArchiveProjectCommand>
{
    public ArchiveProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0)
            .WithMessage("Current user ID must be greater than 0");
    }
}

