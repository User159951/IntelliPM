using FluentValidation;
using IntelliPM.Application.Agents.DTOs;

namespace IntelliPM.Application.Agents.Validators;

/// <summary>
/// Validator for BusinessAgentOutputDto.
/// </summary>
public class BusinessAgentOutputValidator : AbstractValidator<BusinessAgentOutputDto>
{
    public BusinessAgentOutputValidator()
    {
        RuleFor(x => x.Confidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("Confidence score must be between 0.0 and 1.0");

        RuleFor(x => x.ValueDeliverySummary)
            .NotEmpty()
            .WithMessage("ValueDeliverySummary is required")
            .MaximumLength(10000)
            .WithMessage("ValueDeliverySummary cannot exceed 10000 characters");

        RuleFor(x => x.ValueMetrics)
            .Must(metrics => metrics.Count <= 50)
            .WithMessage("Cannot have more than 50 value metrics");

        RuleForEach(x => x.ValueMetrics)
            .Must(kvp => !string.IsNullOrEmpty(kvp.Key))
            .WithMessage("Metric key cannot be empty")
            .Must(kvp => kvp.Key.Length <= 100)
            .WithMessage("Metric key cannot exceed 100 characters");

        RuleFor(x => x.BusinessHighlights)
            .NotEmpty()
            .WithMessage("At least one business highlight is required")
            .Must(highlights => highlights.Count <= 20)
            .WithMessage("Cannot have more than 20 business highlights");

        RuleForEach(x => x.BusinessHighlights)
            .NotEmpty()
            .WithMessage("Business highlight cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Business highlight cannot exceed 1000 characters");

        RuleFor(x => x.StrategicRecommendations)
            .NotEmpty()
            .WithMessage("At least one strategic recommendation is required")
            .Must(recommendations => recommendations.Count <= 20)
            .WithMessage("Cannot have more than 20 strategic recommendations");

        RuleForEach(x => x.StrategicRecommendations)
            .NotEmpty()
            .WithMessage("Strategic recommendation cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Strategic recommendation cannot exceed 1000 characters");
    }
}

