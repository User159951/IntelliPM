using MediatR;

namespace IntelliPM.Application.Notifications.Commands;

public class MarkAllNotificationsReadCommand : IRequest
{
    public int UserId { get; set; }
}
