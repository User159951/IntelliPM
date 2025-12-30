using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IntelliPM.Application.Common.Services;

public interface INotificationService
{
    System.Threading.Tasks.Task CreateNotificationAsync(
        int userId,
        string type,
        string message,
        string? entityType = null,
        int? entityId = null,
        int? projectId = null,
        CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task CreateNotificationAsync(
        int userId,
        string type,
        string message,
        string? entityType = null,
        int? entityId = null,
        int? projectId = null,
        CancellationToken cancellationToken = default)
    {
        var notificationRepo = _unitOfWork.Repository<Notification>();

        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            EntityType = entityType,
            EntityId = entityId,
            ProjectId = projectId,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await notificationRepo.AddAsync(notification, cancellationToken);
    }
}
