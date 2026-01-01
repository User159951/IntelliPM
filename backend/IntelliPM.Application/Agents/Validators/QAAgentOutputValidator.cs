using FluentValidation;
using IntelliPM.Application.Agents.DTOs;

namespace IntelliPM.Application.Agents.Validators;

/// <summary>
/// Validator for QAAgentOutputDto.
/// </summary>
public class QAAgentOutputValidator : AbstractValidator<QAAgentOutputDto>
{
    public QAAgentOutputValidator()
    {
        RuleFor(x => x.Confidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("Confidence score must be between 0.0 and 1.0");

        RuleFor(x => x.DefectAnalysis)
            .NotEmpty()
            .WithMessage("DefectAnalysis is required")
            .MaximumLength(10000)
            .WithMessage("DefectAnalysis cannot exceed 10000 characters");

        RuleFor(x => x.Patterns)
            .Must(patterns => patterns.Count <= 50)
            .WithMessage("Cannot have more than 50 defect patterns");

        RuleForEach(x => x.Patterns)
            .SetValidator(new DefectPatternDtoValidator());

        RuleFor(x => x.Recommendations)
            .NotEmpty()
            .WithMessage("At least one recommendation is required")
            .Must(recommendations => recommendations.Count <= 20)
            .WithMessage("Cannot have more than 20 recommendations");

        RuleForEach(x => x.Recommendations)
            .NotEmpty()
            .WithMessage("Recommendation cannot be empty")
            .MaximumLength(1000)
            .WithMessage("Recommendation cannot exceed 1000 characters");
    }
}

/// <summary>
/// Validator for DefectPatternDto.
/// </summary>
public class DefectPatternDtoValidator : AbstractValidator<DefectPatternDto>
{
    public DefectPatternDtoValidator()
    {
        RuleFor(x => x.Pattern)
            .NotEmpty()
            .WithMessage("Pattern is required")
            .MaximumLength(500)
            .WithMessage("Pattern cannot exceed 500 characters");

        RuleFor(x => x.Frequency)
            .GreaterThan(0)
            .WithMessage("Frequency must be greater than 0");

        RuleFor(x => x.Severity)
            .NotEmpty()
            .WithMessage("Severity is required")
            .Must(severity => new[] { "Critical", "High", "Medium", "Low" }.Contains(severity))
            .WithMessage("Severity must be one of: Critical, High, Medium, Low");

        RuleFor(x => x.Suggestion)
            .NotEmpty()
            .WithMessage("Suggestion is required")
            .MaximumLength(2000)
            .WithMessage("Suggestion cannot exceed 2000 characters");
    }
}

