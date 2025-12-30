using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Identity.Commands;

public class AcceptInviteCommandHandler : IRequestHandler<AcceptInviteCommand, AcceptInviteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AcceptInviteCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _tokenService = tokenService;
    }

    public async Task<AcceptInviteResponse> Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
    {
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

        // Check if user already exists
        var userRepo = _unitOfWork.Repository<User>();
        var existingUser = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Email == invitation.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {invitation.Email} already exists");
        }

        // Generate username from email (before @)
        var username = invitation.Email.Split('@')[0];

        // Create user account
        var userId = await _authService.RegisterAsync(
            username,
            invitation.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            cancellationToken);

        // Get the created user
        var user = await userRepo.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("Failed to create user account");
        }

        // Update user's organization and role
        user.OrganizationId = invitation.OrganizationId;
        user.GlobalRole = invitation.GlobalRole;
        userRepo.Update(user);

        // Add user to project if ProjectId is specified
        if (invitation.ProjectId.HasValue)
        {
            var projectMemberRepo = _unitOfWork.Repository<ProjectMember>();
            var existingMember = await projectMemberRepo.Query()
                .FirstOrDefaultAsync(
                    pm => pm.ProjectId == invitation.ProjectId.Value && pm.UserId == userId,
                    cancellationToken);

            if (existingMember == null)
            {
                await projectMemberRepo.AddAsync(new ProjectMember
                {
                    ProjectId = invitation.ProjectId.Value,
                    UserId = userId,
                    Role = ProjectRole.Developer, // Default role, can be customized
                    InvitedById = invitation.CreatedById,
                    InvitedAt = invitation.CreatedAt.DateTime,
                    JoinedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
        }

        // Mark invitation as used
        invitation.IsUsed = true;
        invitation.UsedAt = DateTimeOffset.UtcNow;
        invitationRepo.Update(invitation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(userId, user.Username, user.Email, new() { user.GlobalRole.ToString() });
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };
        await refreshTokenRepo.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AcceptInviteResponse(
            userId,
            user.Username,
            user.Email,
            accessToken,
            refreshToken
        );
    }
}

