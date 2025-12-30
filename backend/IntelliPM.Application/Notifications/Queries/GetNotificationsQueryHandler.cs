using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Notifications.Queries;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetNotificationsResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notificationRepo = _unitOfWork.Repository<Notification>();

        var baseQuery = notificationRepo.Query()
            .Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly)
        {
            baseQuery = baseQuery.Where(n => !n.IsRead);
        }

        var query = baseQuery.OrderByDescending(n => n.CreatedAt);

        var notifications = await query
            .Take(request.Limit)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                ProjectId = n.ProjectId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Get total unread count
        var unreadCount = await notificationRepo.Query()
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);

        return new GetNotificationsResponse
        {
            Notifications = notifications,
            UnreadCount = unreadCount
        };
    }
}
