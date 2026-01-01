using FluentValidation;
using IntelliPM.Application.Agents.DTOs;

namespace IntelliPM.Application.Agents.Validators;

/// <summary>
/// Validator for DeliveryAgentOutputDto.
/// </summary>
public class DeliveryAgentOutputValidator : AbstractValidator<DeliveryAgentOutputDto>
{
    public DeliveryAgentOutputValidator()
    {
        RuleFor(x => x.Confidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("Confidence score must be between 0.0 and 1.0");

        RuleFor(x => x.RiskAssessment)
            .NotEmpty()
            .WithMessage("RiskAssessment is required")
            .MaximumLength(10000)
            .WithMessage("RiskAssessment cannot exceed 10000 characters");

        RuleFor(x => x.RecommendedActions)
            .NotEmpty()
            .WithMessage("At least one recommended action is required")
            .Must(actions => actions.Count <= 20)
            .WithMessage("Cannot have more than 20 recommended actions");

        RuleForEach(x => x.RecommendedActions)
            .NotEmpty()
            .WithMessage("Recommended action cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Recommended action cannot exceed 1000 characters");

        RuleFor(x => x.Highlights)
            .Must(highlights => highlights.Count <= 20)
            .WithMessage("Cannot have more than 20 highlights");

        RuleForEach(x => x.Highlights)
            .NotEmpty()
            .When(x => x.Highlights.Any())
            .WithMessage("Highlight cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Highlight cannot exceed 1000 characters");
    }
}

