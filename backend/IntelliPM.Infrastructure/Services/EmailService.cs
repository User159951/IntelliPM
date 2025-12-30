using IntelliPM.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Stub email service that logs instead of sending emails
/// Used when SMTP is not configured
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendInvitationEmailAsync(
        string email,
        string invitationToken,
        string invitationLink,
        string? userName = null,
        string? inviterName = null,
        string? projectName = null,
        string? role = null,
        DateTime? expirationDate = null,
        CancellationToken ct = default)
    {
        // Stub implementation - log instead of sending email
        _logger.LogInformation(
            "Email invitation sent to {Email} (User: {UserName}, Project: {ProjectName}, Role: {Role}, Inviter: {InviterName}) " +
            "with token {Token}. Link: {Link}. Expires: {ExpirationDate}",
            email,
            userName ?? email,
            projectName ?? "N/A",
            role ?? "Member",
            inviterName ?? "System",
            invitationToken.Substring(0, Math.Min(10, invitationToken.Length)) + "...",
            invitationLink,
            expirationDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A");
        
        // TODO: Integrate with actual email service (SendGrid, SMTP, etc.)
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(
        string email,
        string resetToken,
        string resetLink,
        string? userName = null,
        string? expirationTime = null,
        CancellationToken ct = default)
    {
        // Stub implementation
        _logger.LogInformation(
            "Password reset email sent to {Email} (User: {UserName}). Link: {Link}. Expires: {ExpirationTime}",
            email,
            userName ?? email,
            resetLink,
            expirationTime ?? "1 hour");
        
        // TODO: Integrate with actual email service
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(
        string email,
        string username,
        string? loginLink = null,
        CancellationToken ct = default)
    {
        // Stub implementation
        _logger.LogInformation(
            "Welcome email sent to {Email} for user {Username}. Login link: {LoginLink}",
            email,
            username,
            loginLink ?? "N/A");
        
        // TODO: Integrate with actual email service
        return Task.CompletedTask;
    }

    public Task<bool> SendOrganizationInvitationEmailAsync(
        string recipientEmail,
        string recipientFirstName,
        string recipientLastName,
        string invitationLink,
        string organizationName,
        string inviterName,
        string role,
        CancellationToken ct = default)
    {
        // Stub implementation - log instead of sending email
        _logger.LogInformation(
            "Organization invitation email sent to {RecipientEmail} (Name: {RecipientFirstName} {RecipientLastName}, " +
            "Organization: {OrganizationName}, Role: {Role}, Inviter: {InviterName}). Link: {InvitationLink}",
            recipientEmail,
            recipientFirstName,
            recipientLastName,
            organizationName,
            role,
            inviterName,
            invitationLink);
        
        // TODO: Integrate with actual email service
        return Task.FromResult(true);
    }

    public Task SendTestEmailAsync(string email, CancellationToken ct = default)
    {
        // Stub implementation
        _logger.LogInformation("Test email sent to {Email}", email);
        return Task.CompletedTask;
    }

    public Task<bool> SendAIQuotaUpdatedEmailAsync(
        string recipientEmail,
        string organizationName,
        string oldTier,
        string newTier,
        CancellationToken ct = default)
    {
        // Stub implementation - log instead of sending email
        _logger.LogInformation(
            "AI quota updated email sent to {RecipientEmail} (Organization: {OrganizationName}, " +
            "Old Tier: {OldTier}, New Tier: {NewTier})",
            recipientEmail,
            organizationName,
            oldTier,
            newTier);

        // TODO: Integrate with actual email service
        return Task.FromResult(true);
    }

    public Task<bool> SendAIDisabledNotificationAsync(
        string recipientEmail,
        string organizationName,
        string reason,
        bool isPermanent,
        CancellationToken ct = default)
    {
        // Stub implementation - log instead of sending email
        _logger.LogWarning(
            "AI disabled notification email sent to {RecipientEmail} (Organization: {OrganizationName}, " +
            "Reason: {Reason}, Permanent: {IsPermanent})",
            recipientEmail,
            organizationName,
            reason,
            isPermanent);

        // TODO: Integrate with actual email service
        return Task.FromResult(true);
    }
}

