using FluentValidation;

namespace IntelliPM.Application.Defects.Commands;

public class CreateDefectCommandValidator : AbstractValidator<CreateDefectCommand>
{
    public CreateDefectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("Valid project ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Defect title is required")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required")
            .Must(severity => new[] { "Low", "Medium", "High", "Critical" }.Contains(severity))
            .WithMessage("Severity must be Low, Medium, High, or Critical");

        RuleFor(x => x.ReportedById)
            .GreaterThan(0).WithMessage("Valid reporter ID is required");
    }
}

