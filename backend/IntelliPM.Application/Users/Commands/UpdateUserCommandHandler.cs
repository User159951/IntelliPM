using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Users.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        // Check permission
        var hasPermission = await _permissionService.HasPermissionAsync(currentUserId, "users.update", cancellationToken);
        if (!hasPermission)
        {
            throw new UnauthorizedException("You don't have permission to update users");
        }

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.OrganizationId == organizationId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Capture old values before updating
        var oldFirstName = user.FirstName;
        var oldLastName = user.LastName;
        var oldEmail = user.Email;
        var oldGlobalRole = user.GlobalRole;
        var changes = new Dictionary<string, string>();

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName != oldFirstName)
        {
            changes["FirstName"] = $"\"{oldFirstName}\" -> \"{request.FirstName}\"";
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName != oldLastName)
        {
            changes["LastName"] = $"\"{oldLastName}\" -> \"{request.LastName}\"";
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Check if email is already taken by another user
            var emailExists = await userRepo.Query()
                .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId && u.OrganizationId == organizationId, cancellationToken);
            
            if (emailExists)
            {
                throw new ValidationException("Email is already taken by another user")
                {
                    Errors = new Dictionary<string, string[]>
                    {
                        { "Email", new[] { "Email is already taken" } }
                    }
                };
            }

            if (request.Email != oldEmail)
            {
                changes["Email"] = $"\"{oldEmail}\" -> \"{request.Email}\"";
                user.Email = request.Email;
            }
        }

        if (request.GlobalRole.HasValue && request.GlobalRole.Value != oldGlobalRole)
        {
            changes["GlobalRole"] = $"{oldGlobalRole} -> {request.GlobalRole.Value}";
            user.GlobalRole = request.GlobalRole.Value;
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create domain event and add to Outbox if there are changes
        if (changes.Count > 0)
        {
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
        }

        return new UpdateUserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.GlobalRole.ToString()
        );
    }
}

