using FluentValidation;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Validator for CreateMilestoneCommand.
/// </summary>
public class CreateMilestoneCommandValidator : AbstractValidator<CreateMilestoneCommand>
{
    public CreateMilestoneCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(MilestoneConstants.Validation.NameRequired)
            .MaximumLength(MilestoneConstants.MaxNameLength)
            .WithMessage(MilestoneConstants.Validation.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(MilestoneConstants.MaxDescriptionLength)
            .WithMessage(MilestoneConstants.Validation.DescriptionMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Milestone type is required")
            .Must(type => type == "Release" || type == "Sprint" || type == "Deadline" || type == "Custom")
            .WithMessage("Milestone type must be one of: Release, Sprint, Deadline, Custom");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage(MilestoneConstants.Validation.DueDateRequired)
            .Must(dueDate => dueDate > DateTimeOffset.UtcNow)
            .WithMessage(MilestoneConstants.Validation.ErrorDueDateInPast);

        RuleFor(x => x.Progress)
            .InclusiveBetween(MilestoneConstants.MinProgress, MilestoneConstants.MaxProgress)
            .WithMessage(MilestoneConstants.Validation.ProgressRange);
    }
}

