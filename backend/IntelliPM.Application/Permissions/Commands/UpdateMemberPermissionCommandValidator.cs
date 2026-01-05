using FluentValidation;

namespace IntelliPM.Application.Permissions.Commands;

/// <summary>
/// Validator for UpdateMemberPermissionCommand.
/// </summary>
public class UpdateMemberPermissionCommandValidator : AbstractValidator<UpdateMemberPermissionCommand>
{
    public UpdateMemberPermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.GlobalRole)
            .Must(role => string.IsNullOrWhiteSpace(role) || 
                          role == "User" || role == "Admin" || role == "SuperAdmin")
            .WithMessage("GlobalRole must be User, Admin, or SuperAdmin")
            .When(x => !string.IsNullOrWhiteSpace(x.GlobalRole));

        RuleFor(x => x.PermissionIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("All permission IDs must be greater than 0")
            .When(x => x.PermissionIds != null && x.PermissionIds.Any());
    }
}

