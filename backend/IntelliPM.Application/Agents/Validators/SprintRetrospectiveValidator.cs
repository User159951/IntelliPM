using FluentValidation;
using IntelliPM.Application.Agents.DTOs;

namespace IntelliPM.Application.Agents.Validators;

/// <summary>
/// Validator for SprintRetrospectiveDto.
/// </summary>
public class SprintRetrospectiveValidator : AbstractValidator<SprintRetrospectiveDto>
{
    public SprintRetrospectiveValidator()
    {
        RuleFor(x => x.SprintId)
            .GreaterThan(0)
            .WithMessage("Sprint ID must be greater than 0");

        RuleFor(x => x.Summary)
            .NotEmpty()
            .WithMessage("Summary is required");

        RuleForEach(x => x.ActionItems)
            .SetValidator(new ActionItemValidator());

        RuleFor(x => x.Metrics)
            .SetValidator(new RetrospectiveMetricsValidator());

        RuleFor(x => x.TeamPerformance)
            .SetValidator(new TeamPerformanceValidator());

        RuleFor(x => x.TeamPerformance.Engagement)
            .Must(engagement => engagement == "low" || engagement == "medium" || engagement == "high")
            .WithMessage("Engagement must be 'low', 'medium', or 'high'");
    }
}

/// <summary>
/// Validator for ActionItemDto.
/// </summary>
public class ActionItemValidator : AbstractValidator<ActionItemDto>
{
    public ActionItemValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty()
            .WithMessage("Action description is required");

        RuleFor(x => x.Priority)
            .Must(priority => priority == "low" || priority == "medium" || priority == "high")
            .WithMessage("Priority must be 'low', 'medium', or 'high'");

        RuleFor(x => x.Owner)
            .NotEmpty()
            .WithMessage("Owner is required");
    }
}

/// <summary>
/// Validator for RetrospectiveMetricsDto.
/// </summary>
public class RetrospectiveMetricsValidator : AbstractValidator<RetrospectiveMetricsDto>
{
    public RetrospectiveMetricsValidator()
    {
        RuleFor(x => x.PlannedStoryPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Planned story points must be >= 0");

        RuleFor(x => x.CompletedStoryPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Completed story points must be >= 0");

        RuleFor(x => x.DefectCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Defect count must be >= 0");

        RuleFor(x => x.CompletionRate)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Completion rate must be between 0 and 1");
    }
}

/// <summary>
/// Validator for TeamPerformanceDto.
/// </summary>
public class TeamPerformanceValidator : AbstractValidator<TeamPerformanceDto>
{
    public TeamPerformanceValidator()
    {
        RuleFor(x => x.Engagement)
            .Must(engagement => engagement == "low" || engagement == "medium" || engagement == "high")
            .WithMessage("Engagement must be 'low', 'medium', or 'high'");
    }
}

