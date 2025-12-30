using MediatR;

namespace IntelliPM.Application.Identity.Commands;

public record AcceptOrganizationInviteCommand(
    string Token,
    string Username,
    string Password,
    string ConfirmPassword
) : IRequest<AcceptInviteResponse>;

