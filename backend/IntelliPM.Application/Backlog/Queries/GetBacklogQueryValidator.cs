using FluentValidation;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.Backlog.Queries;

/// <summary>
/// Validator for GetBacklogQuery.
/// </summary>
public class GetBacklogQueryValidator : AbstractValidator<GetBacklogQuery>
{
    public GetBacklogQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.Priority)
            .Must(priority => string.IsNullOrEmpty(priority) || TaskConstants.Priorities.All.Contains(priority))
            .WithMessage($"Priority must be one of: {string.Join(", ", TaskConstants.Priorities.All)}");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || 
                status == TaskConstants.Statuses.Todo ||
                status == TaskConstants.Statuses.InProgress ||
                status == TaskConstants.Statuses.Done)
            .WithMessage($"Status must be one of: {TaskConstants.Statuses.Todo}, {TaskConstants.Statuses.InProgress}, {TaskConstants.Statuses.Done}");
    }
}

