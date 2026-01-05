using FluentValidation;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Validator for UpsertOrganizationPermissionPolicyCommand.
/// </summary>
public class UpsertOrganizationPermissionPolicyCommandValidator : AbstractValidator<UpsertOrganizationPermissionPolicyCommand>
{
    public UpsertOrganizationPermissionPolicyCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");

        RuleFor(x => x.AllowedPermissions)
            .NotNull()
            .WithMessage("Allowed permissions list cannot be null");

        // Note: Permission name validation (existence check) is done in the handler
        // since it requires database access
    }
}

