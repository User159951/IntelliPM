using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Identity.Queries;

public class ValidateInviteTokenQueryHandler : IRequestHandler<ValidateInviteTokenQuery, ValidateInviteTokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ValidateInviteTokenQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidateInviteTokenResponse> Handle(ValidateInviteTokenQuery request, CancellationToken cancellationToken)
    {
        // First, try to find in OrganizationInvitation (new type)
        var organizationInvitationRepo = _unitOfWork.Repository<OrganizationInvitation>();
        var organizationInvitation = await organizationInvitationRepo.Query()
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);

        if (organizationInvitation != null)
        {
            if (organizationInvitation.IsUsed)
            {
                throw new InvalidOperationException("This invitation has already been used");
            }

            if (organizationInvitation.ExpiresAt < DateTime.UtcNow)
            {
                throw new InvalidOperationException("This invitation has expired");
            }

            // Get organization name
            var organizationRepo = _unitOfWork.Repository<Organization>();
            var organization = await organizationRepo.GetByIdAsync(organizationInvitation.OrganizationId, cancellationToken);
            var organizationName = organization?.Name ?? "Unknown Organization";

            return new ValidateInviteTokenResponse(
                organizationInvitation.Email,
                organizationName
            );
        }

        // Fallback to old Invitation type (for project invitations)
        var invitationRepo = _unitOfWork.Repository<Invitation>();
        var invitation = await invitationRepo.Query()
            .Include(i => i.Organization)
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);

        if (invitation == null)
        {
            throw new NotFoundException("Invalid invitation token");
        }

        if (invitation.IsUsed)
        {
            throw new InvalidOperationException("This invitation has already been used");
        }

        if (invitation.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("This invitation has expired");
        }

        return new ValidateInviteTokenResponse(
            invitation.Email,
            invitation.Organization.Name
        );
    }
}

