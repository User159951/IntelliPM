using FluentValidation;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Validator for AddCommentCommand.
/// </summary>
public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("Entity type is required")
            .Must(type => CommentConstants.ValidEntityTypes.Contains(type))
            .WithMessage($"Entity type must be one of: {string.Join(", ", CommentConstants.ValidEntityTypes)}");

        RuleFor(x => x.EntityId)
            .GreaterThan(0)
            .WithMessage("Entity ID must be greater than 0");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required")
            .MaximumLength(CommentConstants.MaxContentLength)
            .WithMessage($"Comment cannot exceed {CommentConstants.MaxContentLength} characters");

        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0)
            .When(x => x.ParentCommentId.HasValue)
            .WithMessage("Parent comment ID must be greater than 0 when provided");
    }
}

