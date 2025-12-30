using MediatR;

namespace IntelliPM.Application.Identity.Queries;

public record ValidateInviteTokenQuery(string Token) : IRequest<ValidateInviteTokenResponse>;

public record ValidateInviteTokenResponse(
    string Email,
    string OrganizationName
);

