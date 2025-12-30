using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Identity.Commands;

public class AcceptOrganizationInviteCommandHandler : IRequestHandler<AcceptOrganizationInviteCommand, AcceptInviteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AcceptOrganizationInviteCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async System.Threading.Tasks.Task<AcceptInviteResponse> Handle(
        AcceptOrganizationInviteCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate token exists and not expired
        var invitationRepo = _unitOfWork.Repository<OrganizationInvitation>();
        var invitation = await invitationRepo.Query()
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);

        if (invitation == null)
        {
            throw new NotFoundException("Invalid invitation token");
        }

        // 2. Validate invitation not already used
        if (invitation.IsUsed)
        {
            throw new InvalidOperationException("This invitation has already been used");
        }

        // Validate token not expired
        if (invitation.IsExpired())
        {
            throw new InvalidOperationException("This invitation has expired");
        }

        // 3. Validate username not already taken
        var userRepo = _unitOfWork.Repository<User>();
        var existingUsername = await userRepo.Query()
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (existingUsername)
        {
            throw new InvalidOperationException($"Username '{request.Username}' is already taken");
        }

        // Check if user with email already exists
        var existingUser = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Email == invitation.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {invitation.Email} already exists");
        }

        // 4 & 5. Password validation is handled by the validator (min 8 chars, uppercase, lowercase, number)
        // ConfirmPassword matching is also validated by the validator

        // 6. Create User entity
        var (passwordHash, passwordSalt) = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = invitation.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            FirstName = string.Empty, // OrganizationInvitation doesn't store FirstName/LastName
            LastName = string.Empty,  // OrganizationInvitation doesn't store FirstName/LastName
            GlobalRole = invitation.Role,
            OrganizationId = invitation.OrganizationId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await userRepo.AddAsync(user, cancellationToken);

        // 7. Mark invitation as accepted (IsUsed = true, AcceptedAt = now)
        invitation.MarkAsAccepted();
        invitationRepo.Update(invitation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7a. Create domain event and add to Outbox for reliable event publishing
        var userCreatedEvent = new UserCreatedEvent
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.GlobalRole,
            OrganizationId = user.OrganizationId,
            CreatedById = invitation.InvitedById
        };

        var eventType = typeof(UserCreatedEvent).AssemblyQualifiedName ?? typeof(UserCreatedEvent).FullName ?? "UserCreatedEvent";
        var payload = JsonSerializer.Serialize(userCreatedEvent);
        var idempotencyKey = $"UserCreated_{user.Id}_{DateTime.UtcNow.Ticks}";

        var outboxMessage = OutboxMessage.Create(eventType, payload, idempotencyKey);
        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Generate JWT tokens
        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Username,
            user.Email,
            new List<string> { user.GlobalRole.ToString() });

        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };
        await refreshTokenRepo.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Return tokens + user info
        return new AcceptInviteResponse(
            user.Id,
            user.Username,
            user.Email,
            accessToken,
            refreshToken
        );
    }
}

