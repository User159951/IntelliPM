using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IntelliPM.Application.Notifications.Commands;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notificationRepo = _unitOfWork.Repository<Notification>();

        var notification = await notificationRepo.Query()
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification == null)
            throw new InvalidOperationException($"Notification with ID {request.NotificationId} not found or access denied");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notificationRepo.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
