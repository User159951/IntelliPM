using MediatR;

namespace IntelliPM.Application.Identity.Commands;

/// <summary>
/// Command to activate a user account.
/// </summary>
public record ActivateUserCommand(int UserId) : IRequest<ActivateUserResponse>;

/// <summary>
/// Response containing the activated user information.
/// </summary>
public record ActivateUserResponse(int UserId, bool IsActive, string Username, string Email);

