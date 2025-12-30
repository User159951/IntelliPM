using MediatR;

namespace IntelliPM.Application.Identity.Commands;

/// <summary>
/// Command to request a password reset email
/// </summary>
public record RequestPasswordResetCommand(
    string EmailOrUsername
) : IRequest<RequestPasswordResetResponse>;

/// <summary>
/// Response for password reset request (always returns success to prevent email enumeration)
/// </summary>
public record RequestPasswordResetResponse(
    bool Success,
    string Message
);

