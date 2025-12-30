using FluentValidation;

namespace IntelliPM.Application.Identity.Commands;

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("Email or username is required")
            .MaximumLength(255).WithMessage("Email or username must not exceed 255 characters");
    }
}

