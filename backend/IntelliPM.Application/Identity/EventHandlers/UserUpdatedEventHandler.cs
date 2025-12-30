using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.Identity.EventHandlers;

/// <summary>
/// Event handler for UserUpdatedEvent that creates an audit log entry.
/// This handler is idempotent and will skip processing if an audit log already exists.
/// </summary>
public class UserUpdatedEventHandler : INotificationHandler<UserUpdatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserUpdatedEventHandler> _logger;

    public UserUpdatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<UserUpdatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing UserUpdatedEvent for user {UserId} ({Username})",
                notification.UserId,
                notification.Username);

            // Check if audit log already exists for this event (idempotency check)
            // Use a combination of action, entity type, entity ID, and timestamp to identify duplicates
            var auditLogRepo = _unitOfWork.Repository<AuditLog>();
            var existingAuditLog = await auditLogRepo.Query()
                .FirstOrDefaultAsync(
                    a => a.Action == "UserUpdated" &&
                         a.EntityType == "User" &&
                         a.EntityId == notification.UserId &&
                         a.CreatedAt == notification.OccurredOn,
                    cancellationToken);

            if (existingAuditLog != null)
            {
                _logger.LogInformation(
                    "Audit log for UserUpdatedEvent (UserId: {UserId}, OccurredOn: {OccurredOn}) already exists. Skipping duplicate processing.",
                    notification.UserId,
                    notification.OccurredOn);
                return;
            }

            // Serialize changes dictionary to JSON
            var changesJson = JsonSerializer.Serialize(notification.Changes);

            // Create audit log entry
            var auditLog = new AuditLog
            {
                UserId = notification.UpdatedById,
                Action = "UserUpdated",
                EntityType = "User",
                EntityId = notification.UserId,
                EntityName = notification.Username,
                Changes = changesJson,
                CreatedAt = notification.OccurredOn
            };

            await auditLogRepo.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully created audit log for UserUpdatedEvent (UserId: {UserId}, AuditLogId: {AuditLogId}, Changes: {Changes})",
                notification.UserId,
                auditLog.Id,
                string.Join(", ", notification.Changes.Select(c => $"{c.Key}: {c.Value}")));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing UserUpdatedEvent for user {UserId}. Error: {ErrorMessage}",
                notification.UserId,
                ex.Message);
            
            // Re-throw to allow OutboxProcessor to handle retry logic
            throw;
        }
    }
}

