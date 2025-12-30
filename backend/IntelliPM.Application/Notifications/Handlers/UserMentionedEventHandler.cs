using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Notifications.Handlers;

/// <summary>
/// Handler for UserMentionedEvent that creates notifications for mentioned users.
/// Checks user preferences before sending notifications.
/// </summary>
public class UserMentionedEventHandler : INotificationHandler<UserMentionedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserMentionedEventHandler> _logger;

    public UserMentionedEventHandler(
        IUnitOfWork unitOfWork,
        INotificationPreferenceService preferenceService,
        IEmailService emailService,
        ILogger<UserMentionedEventHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _preferenceService = preferenceService ?? throw new ArgumentNullException(nameof(preferenceService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task Handle(UserMentionedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Processing UserMentionedEvent for user {UserId}", notification.MentionedUserId);

            // Check if user wants to receive mention notifications
            var shouldSendInApp = await _preferenceService.ShouldSendNotification(
                notification.MentionedUserId,
                NotificationConstants.Types.Mentioned,
                "inapp",
                ct);

            var shouldSendEmail = await _preferenceService.ShouldSendNotification(
                notification.MentionedUserId,
                NotificationConstants.Types.Mentioned,
                "email",
                ct);

            // Create in-app notification
            if (shouldSendInApp)
            {
                var inAppNotification = new Notification
                {
                    UserId = notification.MentionedUserId,
                    OrganizationId = notification.OrganizationId,
                    Type = NotificationConstants.Types.Mentioned,
                    Message = $"{notification.CommentAuthorName} mentioned you in {notification.EntityType.ToLower()}: {notification.EntityTitle}",
                    EntityType = notification.EntityType,
                    EntityId = notification.EntityId,
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.Repository<Notification>().AddAsync(inAppNotification, ct);
            }

            // Send email notification
            if (shouldSendEmail)
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(notification.MentionedUserId, ct);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    // TODO: Implement SendMentionNotificationEmailAsync in IEmailService
                    // For now, we'll just log that email should be sent
                    _logger.LogInformation(
                        "Email notification should be sent to {Email} for mention in comment {CommentId}",
                        user.Email,
                        notification.CommentId);
                    
                    // Uncomment when IEmailService.SendMentionNotificationEmailAsync is implemented:
                    // await _emailService.SendMentionNotificationEmailAsync(
                    //     user.Email,
                    //     notification.CommentAuthorName,
                    //     notification.EntityType,
                    //     notification.EntityTitle,
                    //     notification.CommentContent,
                    //     ct);
                }
            }

            // Update mention notification status
            var mention = await _unitOfWork.Repository<Mention>()
                .Query()
                .FirstOrDefaultAsync(m => m.CommentId == notification.CommentId
                    && m.MentionedUserId == notification.MentionedUserId, ct);

            if (mention != null)
            {
                mention.NotificationSent = true;
                mention.NotificationSentAt = DateTimeOffset.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("UserMentionedEvent processed successfully for user {UserId}", notification.MentionedUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserMentionedEvent for user {UserId}", notification.MentionedUserId);
            // Don't throw - eventual consistency
            // The OutboxProcessor will retry if needed
        }
    }
}

