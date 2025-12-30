using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Services;

/// <summary>
/// Interface for managing user notification preferences.
/// </summary>
public interface INotificationPreferenceService
{
    /// <summary>
    /// Check if a notification should be sent to a user based on their preferences.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="notificationType">Type of notification</param>
    /// <param name="channel">Channel (email, inapp, push)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if notification should be sent, false otherwise</returns>
    System.Threading.Tasks.Task<bool> ShouldSendNotification(
        int userId,
        string notificationType,
        string channel,
        CancellationToken ct);

    /// <summary>
    /// Initialize default notification preferences for a new user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="ct">Cancellation token</param>
    System.Threading.Tasks.Task InitializeDefaultPreferencesAsync(
        int userId,
        int organizationId,
        CancellationToken ct);
}

/// <summary>
/// Service for managing user notification preferences.
/// Handles preference checking and initialization of default preferences.
/// </summary>
public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationPreferenceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Check if a notification should be sent to a user based on their preferences.
    /// </summary>
    public async System.Threading.Tasks.Task<bool> ShouldSendNotification(
        int userId,
        string notificationType,
        string channel,
        CancellationToken ct)
    {
        var preference = await _unitOfWork.Repository<NotificationPreference>()
            .Query()
            .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == notificationType, ct);

        // Use defaults if preference not found
        if (preference == null)
        {
            if (NotificationConstants.DefaultPreferences.TryGetValue(notificationType, out var defaults))
            {
                return channel.ToLower() switch
                {
                    "email" => defaults.Email,
                    "inapp" => defaults.InApp,
                    "push" => false, // Push not enabled by default
                    _ => false
                };
            }
            return true; // Default to enabled if type not found in defaults
        }

        // Check frequency
        if (preference.Frequency == NotificationConstants.Frequencies.Never)
            return false;

        // Check channel
        return channel.ToLower() switch
        {
            "email" => preference.EmailEnabled,
            "inapp" => preference.InAppEnabled,
            "push" => preference.PushEnabled,
            _ => false
        };
    }

    /// <summary>
    /// Initialize default notification preferences for a new user.
    /// </summary>
    public async System.Threading.Tasks.Task InitializeDefaultPreferencesAsync(
        int userId,
        int organizationId,
        CancellationToken ct)
    {
        var existingPreferences = await _unitOfWork.Repository<NotificationPreference>()
            .Query()
            .Where(np => np.UserId == userId)
            .Select(np => np.NotificationType)
            .ToListAsync(ct);

        foreach (var (notificationType, (email, inApp, frequency)) in NotificationConstants.DefaultPreferences)
        {
            if (!existingPreferences.Contains(notificationType))
            {
                var preference = new NotificationPreference
                {
                    UserId = userId,
                    OrganizationId = organizationId,
                    NotificationType = notificationType,
                    EmailEnabled = email,
                    InAppEnabled = inApp,
                    PushEnabled = false,
                    Frequency = frequency,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.Repository<NotificationPreference>().AddAsync(preference, ct);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}

