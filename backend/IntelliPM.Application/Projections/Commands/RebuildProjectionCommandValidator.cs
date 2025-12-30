using FluentValidation;

namespace IntelliPM.Application.Projections.Commands;

/// <summary>
/// Validator for RebuildProjectionCommand.
/// </summary>
public class RebuildProjectionCommandValidator : AbstractValidator<RebuildProjectionCommand>
{
    public RebuildProjectionCommandValidator()
    {
        RuleFor(x => x.ProjectionType)
            .NotEmpty()
            .WithMessage("ProjectionType is required")
            .Must(type => new[] { "All", "TaskBoard", "SprintSummary", "ProjectOverview" }
                .Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("ProjectionType must be one of: All, TaskBoard, SprintSummary, ProjectOverview");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .When(x => x.ProjectId.HasValue)
            .WithMessage("ProjectId must be greater than 0");

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .When(x => x.OrganizationId.HasValue)
            .WithMessage("OrganizationId must be greater than 0");
    }
}

