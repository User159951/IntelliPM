using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Admin.Commands;

public record InviteOrganizationUserCommand(
    string Email,
    GlobalRole Role,
    string FirstName,
    string LastName
) : IRequest<InviteOrganizationUserResponse>;

public record InviteOrganizationUserResponse(
    Guid InvitationId,
    string Email,
    string InvitationLink
);

