using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Identity.Queries;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;

    public GetCurrentUserQueryHandler(IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userRepo = _unitOfWork.Repository<User>();
        
        var user = await userRepo.Query()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Get user permissions based on their GlobalRole
        var permissions = await _permissionService.GetUserPermissionsAsync(request.UserId, cancellationToken);

        return new CurrentUserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.GlobalRole,
            user.OrganizationId,
            permissions.ToArray()
        );
    }
}

