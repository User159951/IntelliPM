using FluentValidation;

namespace IntelliPM.Application.Attachments.Commands;

/// <summary>
/// Validator for DeleteAttachmentCommand.
/// </summary>
public class DeleteAttachmentCommandValidator : AbstractValidator<DeleteAttachmentCommand>
{
    public DeleteAttachmentCommandValidator()
    {
        RuleFor(x => x.AttachmentId)
            .GreaterThan(0)
            .WithMessage("Attachment ID must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");
    }
}

