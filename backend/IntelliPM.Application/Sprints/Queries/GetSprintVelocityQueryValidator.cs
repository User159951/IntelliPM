using FluentValidation;

namespace IntelliPM.Application.Sprints.Queries;

/// <summary>
/// Validator for GetSprintVelocityQuery.
/// </summary>
public class GetSprintVelocityQueryValidator : AbstractValidator<GetSprintVelocityQuery>
{
    public GetSprintVelocityQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.SprintId)
            .GreaterThan(0)
            .When(x => x.SprintId.HasValue)
            .WithMessage("Sprint ID must be greater than 0 if provided");

        RuleFor(x => x.LastNSprints)
            .InclusiveBetween(1, 20)
            .When(x => x.LastNSprints.HasValue)
            .WithMessage("LastNSprints must be between 1 and 20 if provided");
    }
}

