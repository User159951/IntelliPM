using FluentValidation;
using IntelliPM.Application.Agents.DTOs;

namespace IntelliPM.Application.Agents.Validators;

/// <summary>
/// Validator for ProductAgentOutputDto.
/// </summary>
public class ProductAgentOutputValidator : AbstractValidator<ProductAgentOutputDto>
{
    public ProductAgentOutputValidator()
    {
        RuleFor(x => x.Confidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("Confidence score must be between 0.0 and 1.0");

        RuleFor(x => x.Summary)
            .NotEmpty()
            .WithMessage("Summary is required")
            .MaximumLength(5000)
            .WithMessage("Summary cannot exceed 5000 characters");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one prioritized item is required")
            .Must(items => items.Count <= 20)
            .WithMessage("Cannot have more than 20 prioritized items");

        RuleForEach(x => x.Items)
            .SetValidator(new PrioritizedItemDtoValidator());
    }
}

/// <summary>
/// Validator for PrioritizedItemDto.
/// </summary>
public class PrioritizedItemDtoValidator : AbstractValidator<PrioritizedItemDto>
{
    public PrioritizedItemDtoValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0)
            .WithMessage("ItemId must be greater than 0");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(500)
            .WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.Priority)
            .GreaterThan(0)
            .WithMessage("Priority must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Priority cannot exceed 100");

        RuleFor(x => x.Rationale)
            .NotEmpty()
            .WithMessage("Rationale is required")
            .MaximumLength(2000)
            .WithMessage("Rationale cannot exceed 2000 characters");
    }
}

