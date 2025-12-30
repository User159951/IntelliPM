using FluentValidation;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.Attachments.Commands;

/// <summary>
/// Validator for UploadAttachmentCommand.
/// </summary>
public class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("Entity type is required")
            .Must(type => AttachmentConstants.EntityTypes.Task == type ||
                         AttachmentConstants.EntityTypes.Project == type ||
                         AttachmentConstants.EntityTypes.Comment == type ||
                         AttachmentConstants.EntityTypes.Defect == type)
            .WithMessage($"Entity type must be one of: {AttachmentConstants.EntityTypes.Task}, {AttachmentConstants.EntityTypes.Project}, {AttachmentConstants.EntityTypes.Comment}, {AttachmentConstants.EntityTypes.Defect}");

        RuleFor(x => x.EntityId)
            .GreaterThan(0)
            .WithMessage("Entity ID must be greater than 0");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required")
            .Must(file => file != null && file.Length > 0)
            .WithMessage("File cannot be empty")
            .Must(file => file != null && file.Length <= AttachmentConstants.MaxFileSizeBytes)
            .WithMessage($"File size cannot exceed {AttachmentConstants.MaxFileSizeBytes / (1024 * 1024)} MB")
            .Must(file => file != null && AttachmentConstants.AllowedExtensions.Contains(
                Path.GetExtension(file.FileName).ToLowerInvariant()))
            .WithMessage($"File extension must be one of: {string.Join(", ", AttachmentConstants.AllowedExtensions)}");

        RuleFor(x => x.UploadedById)
            .GreaterThan(0)
            .WithMessage("Uploaded by ID must be greater than 0");

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");
    }
}

