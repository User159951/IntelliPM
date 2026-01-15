using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IntelliPM.Application.Common.Services;

public interface INotificationService
{
    System.Threading.Tasks.Task CreateNotificationAsync(
        int userId,
        int organizationId,
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
        int organizationId,
        string type,
        string message,
        string? entityType = null,
        int? entityId = null,
        int? projectId = null,
        CancellationToken cancellationToken = default)
    {
        var notificationRepo = _unitOfWork.Repository<Notification>();

        // Validate that user exists in the organization
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Id == userId && u.OrganizationId == organizationId, cancellationToken);
        
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} does not exist in organization {organizationId}");
        }

        var notification = new Notification
        {
            UserId = userId,
            OrganizationId = organizationId,
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
