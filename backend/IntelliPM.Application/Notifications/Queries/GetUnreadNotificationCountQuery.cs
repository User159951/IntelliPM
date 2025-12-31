using MediatR;

namespace IntelliPM.Application.Notifications.Queries;

/// <summary>
/// Query to get the count of unread notifications for the current user
/// </summary>
public record GetUnreadNotificationCountQuery : IRequest<GetUnreadNotificationCountResponse>;

/// <summary>
/// Response containing the unread notification count
/// </summary>
public record GetUnreadNotificationCountResponse(int UnreadCount);

