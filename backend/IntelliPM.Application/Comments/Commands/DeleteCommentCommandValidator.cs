using FluentValidation;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Validator for DeleteCommentCommand.
/// </summary>
public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0)
            .WithMessage("Comment ID must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization ID must be greater than 0");
    }
}

