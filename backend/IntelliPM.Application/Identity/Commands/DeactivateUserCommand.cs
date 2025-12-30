using MediatR;

namespace IntelliPM.Application.Identity.Commands;

/// <summary>
/// Command to deactivate a user account.
/// </summary>
public record DeactivateUserCommand(int UserId) : IRequest<DeactivateUserResponse>;

/// <summary>
/// Response containing the deactivated user information.
/// </summary>
public record DeactivateUserResponse(int UserId, bool IsActive, string Username, string Email);

