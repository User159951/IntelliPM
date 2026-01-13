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
/// Handler for activating a user account.
/// Only Admin users can activate other users.
/// </summary>
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, ActivateUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ActivateUserCommandHandler> _logger;

    public ActivateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ActivateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ActivateUserResponse> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
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
                "User {CurrentUserId} attempted to activate user {UserId} without admin permissions",
                currentUserId,
                request.UserId);
            throw new UnauthorizedException("Only administrators can activate users");
        }

        var isSuperAdmin = _currentUserService.IsSuperAdmin();
        
        // SuperAdmin can activate users from any organization, Admin can only activate users from their own organization
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

        // Activate the user
        user.IsActive = true;
        user.UpdatedAt = DateTimeOffset.UtcNow;

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
            "User {UserId} ({Username}) was activated by admin {AdminUserId}",
            user.Id,
            user.Username,
            currentUserId);

        return new ActivateUserResponse(
            user.Id,
            user.IsActive,
            user.Username,
            user.Email);
    }
}

