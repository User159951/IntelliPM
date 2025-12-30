using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace IntelliPM.Application.Identity.Commands;

public class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, InviteUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<InviteUserCommandHandler> _logger;

    public InviteUserCommandHandler(
        IUnitOfWork unitOfWork, 
        IEmailService emailService,
        ILogger<InviteUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<InviteUserResponse> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        // Check if current user is Admin
        var userRepo = _unitOfWork.Repository<User>();
        var currentUser = await userRepo.GetByIdAsync(request.CreatedById, cancellationToken);
        
        if (currentUser == null)
            throw new NotFoundException($"User with ID {request.CreatedById} not found");

        // Only Admins can invite users with Admin role
        if (request.GlobalRole == GlobalRole.Admin && currentUser.GlobalRole != GlobalRole.Admin)
            throw new UnauthorizedException("Only Admins can invite users with Admin role");

        // Check if user with email already exists
        var existingUser = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        if (existingUser != null)
            throw new InvalidOperationException($"User with email {request.Email} already exists");

        // Check if there's already a pending invitation for this email
        var invitationRepo = _unitOfWork.Repository<Invitation>();
        var existingInvitation = await invitationRepo.Query()
            .FirstOrDefaultAsync(
                i => i.Email == request.Email && !i.IsUsed && i.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);
        
        if (existingInvitation != null)
            throw new InvalidOperationException($"A pending invitation already exists for {request.Email}");

        // Validate ProjectId if provided
        if (request.ProjectId.HasValue)
        {
            var projectRepo = _unitOfWork.Repository<Project>();
            var projectExists = await projectRepo.Query()
                .AnyAsync(p => p.Id == request.ProjectId.Value, cancellationToken);
            
            if (!projectExists)
                throw new NotFoundException($"Project with ID {request.ProjectId.Value} not found");
        }

        // Generate secure invitation token
        var token = GenerateSecureToken();

        // Create invitation (expires in 7 days)
        var invitation = new Invitation
        {
            Token = token,
            Email = request.Email,
            GlobalRole = request.GlobalRole,
            ProjectId = request.ProjectId,
            CreatedById = request.CreatedById,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsUsed = false
        };

        await invitationRepo.AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate invitation link (frontend URL)
        // TODO: Get from configuration
        var baseUrl = "http://localhost:3001"; // Should come from configuration
        var invitationLink = $"{baseUrl}/accept-invitation?token={token}";

        // Get project information if ProjectId is provided
        string? projectName = null;
        string? role = null;
        if (request.ProjectId.HasValue)
        {
            var projectRepo = _unitOfWork.Repository<Project>();
            var project = await projectRepo.GetByIdAsync(request.ProjectId.Value, cancellationToken);
            projectName = project?.Name;
            
            // Since this is an invitation, the user doesn't exist yet, so use the GlobalRole as the role
            // When the user accepts the invitation, they will be added to the project with the appropriate role
            role = request.GlobalRole.ToString();
        }
        else
        {
            role = request.GlobalRole.ToString();
        }

        // Get inviter name
        var inviterName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
        if (string.IsNullOrEmpty(inviterName))
            inviterName = currentUser.Username;

        // Send invitation email with full details
        await _emailService.SendInvitationEmailAsync(
            email: request.Email,
            invitationToken: token,
            invitationLink: invitationLink,
            userName: null, // Will use email as fallback
            inviterName: inviterName,
            projectName: projectName,
            role: role,
            expirationDate: invitation.ExpiresAt.DateTime,
            ct: cancellationToken);

        _logger.LogInformation("Invitation created for {Email} with role {Role} by user {CreatedById}", 
            request.Email, request.GlobalRole, request.CreatedById);

        return new InviteUserResponse(
            invitation.Id,
            invitation.Email,
            invitation.Token,
            invitation.ExpiresAt
        );
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        // Use Base64Url encoding (URL-safe base64)
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}

