using MediatR;

namespace IntelliPM.Application.Notifications.Queries;

public class GetNotificationsQuery : IRequest<GetNotificationsResponse>
{
    public int UserId { get; set; }
    public int OrganizationId { get; set; }
    public bool UnreadOnly { get; set; } = false;
    public int Limit { get; set; } = 10;
    public int Offset { get; set; } = 0;
}

public class GetNotificationsResponse
{
    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public int? ProjectId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
