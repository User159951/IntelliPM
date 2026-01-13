using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.Identity.Commands;

/// <summary>
/// Handler for deactivating a user account.
/// Only Admin users can deactivate other users.
/// Users cannot deactivate their own account.
/// </summary>
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, DeactivateUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeactivateUserCommandHandler> _logger;

    public DeactivateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeactivateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DeactivateUserResponse> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        // Check if current user is Admin using GlobalPermissions
        var userRepo = _unitOfWork.Repository<User>();
        var currentUser = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        if (currentUser == null || !GlobalPermissions.CanManageUsers(currentUser.GlobalRole))
        {
            _logger.LogWarning(
                "User {CurrentUserId} attempted to deactivate user {UserId} without admin permissions",
                currentUserId,
                request.UserId);
            throw new UnauthorizedException("Only administrators can deactivate users");
        }

        // Prevent self-deactivation
        if (request.UserId == currentUserId)
        {
            _logger.LogWarning(
                "User {CurrentUserId} attempted to deactivate their own account",
                currentUserId);
            throw new ValidationException("Cannot deactivate your own account");
        }

        var isSuperAdmin = _currentUserService.IsSuperAdmin();
        
        // SuperAdmin can deactivate users from any organization, Admin can only deactivate users from their own organization
        var user = isSuperAdmin
            ? await userRepo.Query()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            : await userRepo.Query()
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.OrganizationId == organizationId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "User {UserId} not found" + (isSuperAdmin ? "" : $" in organization {organizationId}"),
                request.UserId);
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Capture old value before updating
        var oldIsActive = user.IsActive;

        // Deactivate the user
        user.IsActive = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        // Invalidate all refresh tokens for the user (optional enhancement)
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        var refreshTokens = await refreshTokenRepo.Query()
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.IsRevoked = true;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create domain event and add to Outbox for reliable event publishing
        var changes = new Dictionary<string, string>
        {
            { "IsActive", $"{oldIsActive} -> {user.IsActive}" }
        };

        var userUpdatedEvent = new UserUpdatedEvent
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            OrganizationId = user.OrganizationId,
            UpdatedById = currentUserId,
            Changes = changes
        };

        var eventType = typeof(UserUpdatedEvent).AssemblyQualifiedName ?? typeof(UserUpdatedEvent).FullName ?? "UserUpdatedEvent";
        var payload = JsonSerializer.Serialize(userUpdatedEvent);
        var idempotencyKey = $"UserUpdated_{user.Id}_{DateTime.UtcNow.Ticks}";

        var outboxMessage = OutboxMessage.Create(eventType, payload, idempotencyKey);
        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} ({Username}) was deactivated by admin {AdminUserId}. {TokenCount} refresh token(s) invalidated.",
            user.Id,
            user.Username,
            currentUserId,
            refreshTokens.Count);

        return new DeactivateUserResponse(
            user.Id,
            user.IsActive,
            user.Username,
            user.Email);
    }
}

