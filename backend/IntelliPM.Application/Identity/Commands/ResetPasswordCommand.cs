using MediatR;

namespace IntelliPM.Application.Identity.Commands;

/// <summary>
/// Command to reset password using a token
/// </summary>
public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword
) : IRequest<ResetPasswordResponse>;

/// <summary>
/// Response for password reset
/// </summary>
public record ResetPasswordResponse(
    bool Success,
    string Message
);

