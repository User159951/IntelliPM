using FluentValidation;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Validator for UpdateCommentCommand.
/// </summary>
public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0)
            .WithMessage("Comment ID must be greater than 0");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required")
            .MaximumLength(CommentConstants.MaxContentLength)
            .WithMessage($"Comment cannot exceed {CommentConstants.MaxContentLength} characters");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");
    }
}

