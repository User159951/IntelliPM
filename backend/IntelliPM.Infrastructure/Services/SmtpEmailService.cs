using IntelliPM.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// SMTP email service implementation using MailKit
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly EmailTemplateService _templateService;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly bool _useSsl; // Kept for backward compatibility, but not used for socket options
    private readonly string? _secureSocketOptionsOverride; // Optional override from config
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string? _frontendBaseUrl;

    public SmtpEmailService(
        IConfiguration configuration,
        ILogger<SmtpEmailService> logger,
        EmailTemplateService templateService,
        IHostEnvironment environment)
    {
        _configuration = configuration;
        _logger = logger;
        _templateService = templateService;

        var emailSection = configuration.GetSection("Email");
        _smtpHost = emailSection["SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = emailSection.GetValue<int>("SmtpPort", 587);
        _smtpUsername = emailSection["SmtpUsername"] ?? string.Empty;
        _smtpPassword = emailSection["SmtpPassword"] ?? string.Empty;
        _useSsl = emailSection.GetValue<bool>("UseSsl", true); // Kept for backward compatibility
        _secureSocketOptionsOverride = emailSection["SecureSocketOptions"]; // Optional override
        _fromEmail = emailSection["FromEmail"] ?? "noreply@intellipm.com";
        _fromName = emailSection["FromName"] ?? "IntelliPM";
        
        // Get frontend URL for email links (not from AllowedOrigins, which is for CORS only)
        _frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3001";
        // Normalize: remove trailing slash if present
        _frontendBaseUrl = _frontendBaseUrl.TrimEnd('/');

        // Warn if using Gmail with mismatched FromEmail and SmtpUsername
        if (!string.IsNullOrEmpty(_smtpHost) && 
            _smtpHost.Contains("gmail.com", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(_smtpUsername) &&
            !string.IsNullOrEmpty(_fromEmail) &&
            !_fromEmail.Equals(_smtpUsername, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Gmail SMTP detected with FromEmail ({FromEmail}) different from SmtpUsername ({SmtpUsername}). " +
                "Gmail may reject or mark emails as spam. Consider setting FromEmail to match SmtpUsername.",
                _fromEmail, _smtpUsername);
        }

        // Warn if using Brevo - FromEmail must be verified in Brevo dashboard
        if (!string.IsNullOrEmpty(_smtpHost) && 
            (_smtpHost.Contains("brevo.com", StringComparison.OrdinalIgnoreCase) || 
             _smtpHost.Contains("sendinblue.com", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogInformation(
                "Brevo SMTP detected. Make sure the FromEmail ({FromEmail}) is verified in your Brevo account. " +
                "Unverified sender addresses will cause emails to be rejected.",
                _fromEmail);
        }
    }

    public async Task SendInvitationEmailAsync(
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
        try
        {
            var variables = new Dictionary<string, string>
            {
                { "UserName", userName ?? email },
                { "InviterName", inviterName ?? "A team member" },
                { "ProjectName", projectName ?? "the project" },
                { "Role", role ?? "Member" },
                { "InvitationLink", invitationLink },
                { "ExpirationDate", expirationDate?.ToString("MMMM dd, yyyy 'at' HH:mm") ?? "30 days" }
            };

            var htmlBody = _templateService.ProcessTemplate("MemberInvitation", variables);
            var subject = $"Invitation to join {projectName ?? "IntelliPM Project"}";

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation("Invitation email sent successfully to {Email} for project {ProjectName}", 
                email, projectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email to {Email}", email);
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(
        string email,
        string resetToken,
        string resetLink,
        string? userName = null,
        string? expirationTime = null,
        CancellationToken ct = default)
    {
        try
        {
            var variables = new Dictionary<string, string>
            {
                { "UserName", userName ?? email },
                { "ResetLink", resetLink },
                { "ExpirationTime", expirationTime ?? "1 hour" }
            };

            var htmlBody = _templateService.ProcessTemplate("PasswordReset", variables);
            var subject = "IntelliPM - Password Reset Request";

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation("Password reset email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(
        string email,
        string username,
        string? loginLink = null,
        CancellationToken ct = default)
    {
        try
        {
            var variables = new Dictionary<string, string>
            {
                { "UserName", username },
                { "LoginLink", loginLink ?? $"{_frontendBaseUrl}/login" }
            };

            var htmlBody = _templateService.ProcessTemplate("Welcome", variables);
            var subject = "Welcome to IntelliPM!";

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation("Welcome email sent successfully to {Email} for user {Username}", email, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            throw;
        }
    }

    public async Task<bool> SendOrganizationInvitationEmailAsync(
        string recipientEmail,
        string recipientFirstName,
        string recipientLastName,
        string invitationLink,
        string organizationName,
        string inviterName,
        string role,
        CancellationToken ct = default)
    {
        try
        {
            var variables = new Dictionary<string, string>
            {
                { "FirstName", recipientFirstName },
                { "LastName", recipientLastName },
                { "Email", recipientEmail },
                { "InvitationLink", invitationLink },
                { "OrganizationName", organizationName },
                { "InviterName", inviterName },
                { "Role", role }
            };

            var htmlBody = _templateService.ProcessTemplate("OrganizationInvitation", variables);
            var subject = $"Invitation à rejoindre {organizationName} sur IntelliPM";

            await SendEmailAsync(recipientEmail, subject, htmlBody, ct);
            
            _logger.LogInformation(
                "Organization invitation email sent successfully to {RecipientEmail} for {RecipientFirstName} {RecipientLastName} " +
                "with role {Role} in organization {OrganizationName}",
                recipientEmail,
                recipientFirstName,
                recipientLastName,
                role,
                organizationName);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send organization invitation email to {RecipientEmail}", recipientEmail);
            return false;
        }
    }

    public async Task SendTestEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var subject = "IntelliPM - Test Email";
            var htmlBody = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #2563eb;'>Test Email from IntelliPM</h2>
                        <p>This is a test email to verify your SMTP configuration.</p>
                        <p>If you received this email, your email settings are configured correctly!</p>
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #e5e7eb;' />
                        <p style='color: #6b7280; font-size: 12px;'>
                            This is an automated test email sent from the IntelliPM administration panel.
                        </p>
                    </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation("Test email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email to {Email}", email);
            throw;
        }
    }

    public async Task<bool> SendAIQuotaUpdatedEmailAsync(
        string recipientEmail,
        string organizationName,
        string oldTier,
        string newTier,
        CancellationToken ct = default)
    {
        try
        {
            var tierChangeText = oldTier == "None" 
                ? $"Your organization has been upgraded to the <strong>{newTier}</strong> tier."
                : $"Your organization's AI quota tier has been changed from <strong>{oldTier}</strong> to <strong>{newTier}</strong>.";

            var subject = $"IntelliPM - AI Quota Updated for {organizationName}";
            var htmlBody = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #2563eb;'>AI Quota Updated</h2>
                        <p>Hello,</p>
                        <p>{tierChangeText}</p>
                        <p>This change affects the AI usage limits for your organization. You can view your current quota status in the IntelliPM dashboard.</p>
                        <div style='background-color: #f3f4f6; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Organization:</strong> {organizationName}</p>
                            <p style='margin: 5px 0 0 0;'><strong>Previous Tier:</strong> {oldTier}</p>
                            <p style='margin: 5px 0 0 0;'><strong>New Tier:</strong> {newTier}</p>
                        </div>
                        <p>If you have any questions about this change, please contact your system administrator.</p>
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #e5e7eb;' />
                        <p style='color: #6b7280; font-size: 12px;'>
                            This is an automated notification from IntelliPM.
                        </p>
                    </body>
                </html>";

            await SendEmailAsync(recipientEmail, subject, htmlBody, ct);
            
            _logger.LogInformation(
                "AI quota updated email sent successfully to {RecipientEmail} for organization {OrganizationName} " +
                "(Old Tier: {OldTier}, New Tier: {NewTier})",
                recipientEmail,
                organizationName,
                oldTier,
                newTier);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send AI quota updated email to {RecipientEmail}", recipientEmail);
            return false;
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        // If SMTP is not configured (empty credentials), log and skip sending
        if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogWarning(
                "SMTP credentials not configured. Email would be sent to {ToEmail} with subject: {Subject}. " +
                "Please configure Email:SmtpUsername and Email:SmtpPassword in appsettings.json",
                toEmail, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress(string.Empty, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            // Disable certificate revocation checking for better compatibility with SMTP servers
            // Some SMTP servers (like Brevo) may have issues with certificate revocation checking
            client.CheckCertificateRevocation = false;

            // Determine socket options: use override if provided and valid, otherwise use port-based logic
            var socketOptions = SmtpSocketOptionsHelper.GetSecureSocketOptions(
                _smtpPort, 
                _secureSocketOptionsOverride, 
                _logger);

            _logger.LogDebug("Connecting to SMTP server {Host}:{Port} with socket options {SocketOptions}", 
                _smtpHost, _smtpPort, socketOptions);

            await client.ConnectAsync(_smtpHost, _smtpPort, socketOptions, ct);
            
            _logger.LogDebug("Connected to SMTP server. Authenticating with username {Username}", _smtpUsername);
            
            // Authenticate if credentials are provided
            if (!string.IsNullOrEmpty(_smtpUsername) && !string.IsNullOrEmpty(_smtpPassword))
            {
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword, ct);
                _logger.LogDebug("SMTP authentication successful");
            }

            _logger.LogDebug("Sending email to {ToEmail} with subject: {Subject}", toEmail, subject);
            await client.SendAsync(message, ct);
            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to send email via SMTP to {ToEmail}. Host: {Host}, Port: {Port}, Username: {Username}, FromEmail: {FromEmail}. Error: {ErrorMessage}", 
                toEmail, _smtpHost, _smtpPort, _smtpUsername, _fromEmail, ex.Message);
            throw;
        }
    }

    public async Task<bool> SendAIDisabledNotificationAsync(
        string recipientEmail,
        string organizationName,
        string reason,
        bool isPermanent,
        CancellationToken ct = default)
    {
        try
        {
            var modeText = isPermanent ? "permanently" : "temporarily";
            var subject = $"IntelliPM - AI Features {modeText} Disabled for {organizationName}";
            var htmlBody = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #dc2626;'>AI Features Disabled</h2>
                        <p>Hello,</p>
                        <p>This is an important notification regarding your organization's AI features on IntelliPM.</p>
                        <div style='background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 15px; margin: 20px 0;'>
                            <p style='margin: 0; color: #991b1b;'><strong>AI features have been {modeText} disabled</strong> for your organization.</p>
                        </div>
                        <div style='background-color: #f3f4f6; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Organization:</strong> {organizationName}</p>
                            <p style='margin: 5px 0 0 0;'><strong>Status:</strong> {modeText} Disabled</p>
                            <p style='margin: 5px 0 0 0;'><strong>Reason:</strong> {reason}</p>
                        </div>
                        <p><strong>What this means:</strong></p>
                        <ul>
                            <li>All AI agent operations are currently blocked</li>
                            <li>AI decision-making features are unavailable</li>
                            <li>Historical AI data and decisions are preserved</li>
                            <li>You can still access all other IntelliPM features</li>
                        </ul>
                        <p>If you have any questions or concerns about this action, please contact your system administrator or IntelliPM support immediately.</p>
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #e5e7eb;' />
                        <p style='color: #6b7280; font-size: 12px;'>
                            This is an automated critical notification from IntelliPM.
                        </p>
                    </body>
                </html>";

            await SendEmailAsync(recipientEmail, subject, htmlBody, ct);

            _logger.LogWarning(
                "AI disabled notification email sent successfully to {RecipientEmail} for organization {OrganizationName} " +
                "(Reason: {Reason}, Permanent: {IsPermanent})",
                recipientEmail,
                organizationName,
                reason,
                isPermanent);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send AI disabled notification email to {RecipientEmail}", recipientEmail);
            return false;
        }
    }

}

