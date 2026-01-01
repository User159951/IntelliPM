using FluentValidation;
using IntelliPM.Application.Agents.DTOs;

namespace IntelliPM.Application.Agents.Validators;

/// <summary>
/// Validator for ManagerAgentOutputDto.
/// </summary>
public class ManagerAgentOutputValidator : AbstractValidator<ManagerAgentOutputDto>
{
    public ManagerAgentOutputValidator()
    {
        RuleFor(x => x.Confidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("Confidence score must be between 0.0 and 1.0");

        RuleFor(x => x.ExecutiveSummary)
            .NotEmpty()
            .WithMessage("ExecutiveSummary is required")
            .MaximumLength(10000)
            .WithMessage("ExecutiveSummary cannot exceed 10000 characters");

        RuleFor(x => x.KeyDecisions)
            .NotEmpty()
            .WithMessage("At least one key decision is required")
            .Must(decisions => decisions.Count <= 20)
            .WithMessage("Cannot have more than 20 key decisions");

        RuleForEach(x => x.KeyDecisions)
            .NotEmpty()
            .WithMessage("Key decision cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Key decision cannot exceed 1000 characters");

        RuleFor(x => x.Highlights)
            .NotEmpty()
            .WithMessage("At least one highlight is required")
            .Must(highlights => highlights.Count <= 20)
            .WithMessage("Cannot have more than 20 highlights");

        RuleForEach(x => x.Highlights)
            .NotEmpty()
            .WithMessage("Highlight cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Highlight cannot exceed 1000 characters");
    }
}

