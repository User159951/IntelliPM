using FluentValidation;

namespace IntelliPM.Application.Projects.Commands;

/// <summary>
/// Validator for CreateProjectCommand.
/// Validates project creation with comprehensive business rules.
/// </summary>
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    private static readonly string[] ValidStatuses = { "Active", "OnHold", "Archived", "Completed" };
    private static readonly string[] ValidTypes = { "Scrum", "Kanban" };

    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MinimumLength(3)
            .WithMessage("Project name must be at least 3 characters")
            .MaximumLength(200)
            .WithMessage("Project name must not exceed 200 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Project name cannot be whitespace only");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Project description is required")
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Project type is required")
            .Must(type => ValidTypes.Contains(type))
            .WithMessage($"Project type must be one of: {string.Join(", ", ValidTypes)}");

        RuleFor(x => x.SprintDurationDays)
            .GreaterThan(0)
            .WithMessage("Sprint duration must be greater than 0")
            .LessThanOrEqualTo(30)
            .WithMessage("Sprint duration must not exceed 30 days");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0)
            .WithMessage("Valid owner ID is required");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.StartDate)
            .Must(date => !date.HasValue || date.Value >= DateTimeOffset.UtcNow.AddYears(-1))
            .WithMessage("Start date cannot be more than 1 year in the past")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.MemberIds)
            .Must(memberIds => memberIds == null || memberIds.All(id => id > 0))
            .WithMessage("All member IDs must be greater than 0")
            .When(x => x.MemberIds != null && x.MemberIds.Any());
    }
}

