using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, DeleteUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;

    public DeleteUserCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    public async Task<DeleteUserResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        // Check permission
        var hasPermission = await _permissionService.HasPermissionAsync(currentUserId, "users.delete", cancellationToken);
        if (!hasPermission)
        {
            throw new UnauthorizedException("You don't have permission to delete users");
        }

        // Prevent self-deletion
        if (request.UserId == currentUserId)
        {
            throw new ValidationException("You cannot delete your own account")
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "UserId", new[] { "You cannot delete your own account" } }
                }
            };
        }

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.OrganizationId == organizationId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Soft delete: set IsActive to false
        user.IsActive = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteUserResponse(true);
    }
}

