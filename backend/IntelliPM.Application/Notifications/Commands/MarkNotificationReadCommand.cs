using MediatR;

namespace IntelliPM.Application.Notifications.Commands;

public class MarkNotificationReadCommand : IRequest
{
    public int NotificationId { get; set; }
    public int UserId { get; set; } // Ensure user can only mark their own notifications as read
}
