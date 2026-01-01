using FluentValidation;
using IntelliPM.Application.Agents.DTOs;

namespace IntelliPM.Application.Agents.Validators;

/// <summary>
/// Validator for TaskDependencyAnalysisDto.
/// </summary>
public class TaskDependencyAnalysisValidator : AbstractValidator<TaskDependencyAnalysisDto>
{
    public TaskDependencyAnalysisValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0");

        RuleFor(x => x.BottleneckRisk)
            .Must(risk => risk == "low" || risk == "medium" || risk == "high")
            .WithMessage("Bottleneck risk must be 'low', 'medium', or 'high'");

        RuleForEach(x => x.DirectDependencies)
            .SetValidator(new DirectDependencyValidator());

        RuleForEach(x => x.CircularDependencies)
            .Must(cycle => cycle != null && cycle.Count > 0)
            .WithMessage("Circular dependency cycles must contain at least one task ID");
    }
}

/// <summary>
/// Validator for DirectDependencyDto.
/// </summary>
public class DirectDependencyValidator : AbstractValidator<DirectDependencyDto>
{
    public DirectDependencyValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Dependency type is required")
            .Must(type => type == "blocks" || type == "blocked_by" || type == "depends_on")
            .WithMessage("Dependency type must be 'blocks', 'blocked_by', or 'depends_on'");
    }
}

