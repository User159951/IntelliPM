using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Users.Commands;

public class BulkUpdateUsersStatusCommandHandler : IRequestHandler<BulkUpdateUsersStatusCommand, BulkUpdateUsersStatusResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;

    public BulkUpdateUsersStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    public async Task<BulkUpdateUsersStatusResponse> Handle(BulkUpdateUsersStatusCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        // Check permission
        var hasPermission = await _permissionService.HasPermissionAsync(currentUserId, "users.update", cancellationToken);
        if (!hasPermission)
        {
            throw new UnauthorizedException("You don't have permission to update users");
        }

        if (request.UserIds == null || request.UserIds.Count == 0)
        {
            throw new ValidationException("User IDs list cannot be empty");
        }

        // Prevent self-deactivation if trying to deactivate
        if (!request.IsActive && request.UserIds.Contains(currentUserId))
        {
            throw new ValidationException("You cannot deactivate your own account");
        }

        var userRepo = _unitOfWork.Repository<User>();
        var users = await userRepo.Query()
            .Where(u => request.UserIds.Contains(u.Id) && u.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var errors = new List<string>();
        int successCount = 0;

        foreach (var user in users)
        {
            try
            {
                user.IsActive = request.IsActive;
                user.UpdatedAt = DateTimeOffset.UtcNow;
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to update user {user.Id}: {ex.Message}");
            }
        }

        // Handle users not found
        var foundUserIds = users.Select(u => u.Id).ToList();
        var notFoundIds = request.UserIds.Except(foundUserIds).ToList();
        foreach (var id in notFoundIds)
        {
            errors.Add($"User with ID {id} not found");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BulkUpdateUsersStatusResponse(
            SuccessCount: successCount,
            FailureCount: errors.Count,
            Errors: errors
        );
    }
}

