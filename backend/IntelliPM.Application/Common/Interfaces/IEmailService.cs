namespace IntelliPM.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendInvitationEmailAsync(
        string email, 
        string invitationToken, 
        string invitationLink,
        string? userName = null,
        string? inviterName = null,
        string? projectName = null,
        string? role = null,
        DateTime? expirationDate = null,
        CancellationToken ct = default);
    
    Task SendPasswordResetEmailAsync(
        string email, 
        string resetToken, 
        string resetLink,
        string? userName = null,
        string? expirationTime = null,
        CancellationToken ct = default);
    
    Task SendWelcomeEmailAsync(
        string email, 
        string username,
        string? loginLink = null,
        CancellationToken ct = default);
    
    Task<bool> SendOrganizationInvitationEmailAsync(
        string recipientEmail,
        string recipientFirstName,
        string recipientLastName,
        string invitationLink,
        string organizationName,
        string inviterName,
        string role,
        CancellationToken ct = default);
    
    Task SendTestEmailAsync(
        string email,
        CancellationToken ct = default);

    /// <summary>
    /// Sends email notification when AI quota is updated for an organization.
    /// </summary>
    Task<bool> SendAIQuotaUpdatedEmailAsync(
        string recipientEmail,
        string organizationName,
        string oldTier,
        string newTier,
        CancellationToken ct = default);

    /// <summary>
    /// Sends email notification when AI features are disabled for an organization.
    /// </summary>
    Task<bool> SendAIDisabledNotificationAsync(
        string recipientEmail,
        string organizationName,
        string reason,
        bool isPermanent,
        CancellationToken ct = default);
}

