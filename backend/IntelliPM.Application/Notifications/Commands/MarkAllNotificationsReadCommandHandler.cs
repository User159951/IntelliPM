using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IntelliPM.Application.Notifications.Commands;

public class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var notificationRepo = _unitOfWork.Repository<Notification>();

        var unreadNotifications = await notificationRepo.Query()
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notificationRepo.Update(notification);
        }

        if (unreadNotifications.Any())
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
