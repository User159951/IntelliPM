using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Notifications.Queries;

/// <summary>
/// Handler for GetUnreadNotificationCountQuery
/// Counts unread notifications for the current user in their organization
/// </summary>
public class GetUnreadNotificationCountQueryHandler 
    : IRequestHandler<GetUnreadNotificationCountQuery, GetUnreadNotificationCountResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadNotificationCountQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<GetUnreadNotificationCountResponse> Handle(
        GetUnreadNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (userId == 0 || organizationId == 0)
        {
            return new GetUnreadNotificationCountResponse(0);
        }

        var unreadCount = await _unitOfWork.Repository<Notification>()
            .Query()
            .AsNoTracking()
            .Where(n => n.UserId == userId 
                     && n.OrganizationId == organizationId 
                     && !n.IsRead)
            .CountAsync(cancellationToken);

        return new GetUnreadNotificationCountResponse(unreadCount);
    }
}

