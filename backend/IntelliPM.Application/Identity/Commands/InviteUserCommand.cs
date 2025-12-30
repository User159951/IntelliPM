using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Identity.Commands;

public record InviteUserCommand(
    string Email,
    GlobalRole GlobalRole,
    int? ProjectId,
    int CreatedById
) : IRequest<InviteUserResponse>;

public record InviteUserResponse(
    int InvitationId,
    string Email,
    string Token,
    DateTimeOffset ExpiresAt
);

