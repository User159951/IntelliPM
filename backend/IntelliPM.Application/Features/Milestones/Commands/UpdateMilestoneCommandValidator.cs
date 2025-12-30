using FluentValidation;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Validator for UpdateMilestoneCommand.
/// </summary>
public class UpdateMilestoneCommandValidator : AbstractValidator<UpdateMilestoneCommand>
{
    public UpdateMilestoneCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Milestone ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(MilestoneConstants.Validation.NameRequired)
            .MaximumLength(MilestoneConstants.MaxNameLength)
            .WithMessage(MilestoneConstants.Validation.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(MilestoneConstants.MaxDescriptionLength)
            .WithMessage(MilestoneConstants.Validation.DescriptionMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage(MilestoneConstants.Validation.DueDateRequired);

        RuleFor(x => x.Progress)
            .InclusiveBetween(MilestoneConstants.MinProgress, MilestoneConstants.MaxProgress)
            .WithMessage(MilestoneConstants.Validation.ProgressRange);
    }
}

