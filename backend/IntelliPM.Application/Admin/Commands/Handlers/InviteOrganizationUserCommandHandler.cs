using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Admin.Commands.Handlers;

public class InviteOrganizationUserCommandHandler : IRequestHandler<InviteOrganizationUserCommand, InviteOrganizationUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InviteOrganizationUserCommandHandler> _logger;

    public InviteOrganizationUserCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        ILogger<InviteOrganizationUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _currentUserService = currentUserService;
        _configuration = configuration;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<InviteOrganizationUserResponse> Handle(
        InviteOrganizationUserCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Permission check: Only Admin can invite
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can invite users to the organization");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new InvalidOperationException("Unable to determine organization");
        }

        // 2. Validate email not already invited (pending invitation)
        var invitationRepo = _unitOfWork.Repository<OrganizationInvitation>();
        var existingPendingInvitation = await invitationRepo.Query()
            .FirstOrDefaultAsync(
                i => i.Email == request.Email 
                    && i.OrganizationId == organizationId 
                    && !i.IsUsed 
                    && i.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (existingPendingInvitation != null)
        {
            throw new InvalidOperationException($"A pending invitation already exists for {request.Email}");
        }

        // 3. Validate email not already registered
        var userRepo = _unitOfWork.Repository<User>();
        var existingUser = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {request.Email} is already registered");
        }

        // 4. Create OrganizationInvitation with secure token (32 bytes, cryptographically secure)
        // The Create method generates the token and sets expiration to 72 hours
        var invitation = OrganizationInvitation.Create(
            email: request.Email,
            role: request.Role,
            organizationId: organizationId,
            invitedById: currentUserId);

        await invitationRepo.AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Build invitation link: $"{frontendUrl}/invite/accept/{token}"
        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3001";
        // Normalize: remove trailing slash if present
        frontendUrl = frontendUrl.TrimEnd('/');
        var invitationLink = $"{frontendUrl}/invite/accept/{invitation.Token}";

        // Get inviter name for email
        var currentUser = await userRepo.GetByIdAsync(currentUserId, cancellationToken);
        var inviterName = currentUser != null 
            ? $"{currentUser.FirstName} {currentUser.LastName}".Trim()
            : null;
        if (string.IsNullOrEmpty(inviterName) && currentUser != null)
        {
            inviterName = currentUser.Username;
        }
        if (string.IsNullOrEmpty(inviterName))
        {
            inviterName = "Un administrateur";
        }

        // Get organization name
        var organizationRepo = _unitOfWork.Repository<Organization>();
        var organization = await organizationRepo.GetByIdAsync(organizationId, cancellationToken);
        var organizationName = organization?.Name ?? "votre organisation";

        // 9. Send invitation email
        try
        {
            var emailSent = await _emailService.SendOrganizationInvitationEmailAsync(
                recipientEmail: request.Email,
                recipientFirstName: request.FirstName,
                recipientLastName: request.LastName,
                invitationLink: invitationLink,
                organizationName: organizationName,
                inviterName: inviterName,
                role: request.Role.ToString(),
                ct: cancellationToken);

            if (!emailSent)
            {
                _logger.LogWarning(
                    "Failed to send invitation email to {Email}, but invitation was created. Invitation ID: {InvitationId}",
                    request.Email,
                    invitation.Id);
            }
            else
            {
                _logger.LogInformation(
                    "Organization invitation email sent successfully to {Email} with role {Role} by user {InvitedById} in organization {OrganizationId}",
                    request.Email,
                    request.Role,
                    currentUserId,
                    organizationId);
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the invitation creation
            // The invitation is already saved, so the user can still accept it via the link
            _logger.LogError(ex,
                "Error sending invitation email to {Email}, but invitation was created. Invitation ID: {InvitationId}. Error: {ErrorMessage}",
                request.Email,
                invitation.Id,
                ex.Message);
            
            // Continue - the invitation is still valid even if email fails
        }

        _logger.LogInformation(
            "Organization invitation created for {Email} with role {Role} by user {InvitedById} in organization {OrganizationId}. Invitation ID: {InvitationId}",
            request.Email,
            request.Role,
            currentUserId,
            organizationId,
            invitation.Id);

        // 10. Return response with invitation ID and link
        return new InviteOrganizationUserResponse(
            InvitationId: invitation.Id,
            Email: invitation.Email,
            InvitationLink: invitationLink);
    }
}

