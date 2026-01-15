using FluentValidation;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Validator for DeleteOrganizationCommand.
/// </summary>
public class DeleteOrganizationCommandValidator : AbstractValidator<DeleteOrganizationCommand>
{
    public DeleteOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");
    }
}
