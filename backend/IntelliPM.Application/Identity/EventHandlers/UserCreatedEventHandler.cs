using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.Identity.EventHandlers;

/// <summary>
/// Event handler for UserCreatedEvent that creates an audit log entry.
/// This handler is idempotent and will skip processing if an audit log already exists.
/// </summary>
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<UserCreatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing UserCreatedEvent for user {UserId} ({Username})",
                notification.UserId,
                notification.Username);

            // Check if audit log already exists (idempotency check)
            var auditLogRepo = _unitOfWork.Repository<AuditLog>();
            var existingAuditLog = await auditLogRepo.Query()
                .FirstOrDefaultAsync(
                    a => a.Action == "UserCreated" &&
                         a.EntityType == "User" &&
                         a.EntityId == notification.UserId,
                    cancellationToken);

            if (existingAuditLog != null)
            {
                _logger.LogInformation(
                    "Audit log for UserCreatedEvent (UserId: {UserId}) already exists. Skipping duplicate processing.",
                    notification.UserId);
                return;
            }

            // Create audit log details as JSON
            var details = JsonSerializer.Serialize(new
            {
                Username = notification.Username,
                Email = notification.Email,
                Role = notification.Role.ToString(),
                OrganizationId = notification.OrganizationId
            });

            // Create audit log entry
            var auditLog = new AuditLog
            {
                UserId = notification.CreatedById,
                Action = "UserCreated",
                EntityType = "User",
                EntityId = notification.UserId,
                EntityName = notification.Username,
                Changes = details,
                CreatedAt = notification.OccurredOn
            };

            await auditLogRepo.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully created audit log for UserCreatedEvent (UserId: {UserId}, AuditLogId: {AuditLogId})",
                notification.UserId,
                auditLog.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing UserCreatedEvent for user {UserId}. Error: {ErrorMessage}",
                notification.UserId,
                ex.Message);
            
            // Re-throw to allow OutboxProcessor to handle retry logic
            throw;
        }
    }
}

