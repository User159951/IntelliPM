using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Email service implementation using System.Net.Mail.SmtpClient
/// Sends emails via SMTP (Brevo) with graceful degradation
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly bool _enableSsl;
    private readonly int _timeoutSeconds = 30;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var emailSection = configuration.GetSection("Email");
        _smtpHost = emailSection["SmtpHost"] ?? string.Empty;
        _smtpPort = emailSection.GetValue<int>("SmtpPort", 587);
        _smtpUsername = emailSection["SmtpUsername"] ?? string.Empty;
        _smtpPassword = emailSection["SmtpPassword"] ?? string.Empty;
        _fromEmail = emailSection["FromEmail"] ?? "noreply@intellipm.com";
        _fromName = emailSection["FromName"] ?? "IntelliPM";
        _enableSsl = emailSection.GetValue<bool>("EnableSsl", true);

        // Validate SMTP configuration at startup
        ValidateSmtpConfiguration();

        _logger.LogInformation(
            "EmailService initialized with SMTP host: {SmtpHost}, Port: {SmtpPort}, FromEmail: {FromEmail}",
            _smtpHost, _smtpPort, _fromEmail);
    }

    /// <summary>
    /// Validates SMTP configuration at startup.
    /// Logs warnings for missing configuration or throws exception if fail-fast is enabled.
    /// </summary>
    private void ValidateSmtpConfiguration()
    {
        var emailSection = _configuration.GetSection("Email");
        var failFast = emailSection.GetValue<bool>("FailFastOnInvalidConfig", false);

        var missingConfig = new List<string>();
        
        if (string.IsNullOrWhiteSpace(_smtpHost))
            missingConfig.Add("Email:SmtpHost");
        
        if (string.IsNullOrWhiteSpace(_smtpUsername))
            missingConfig.Add("Email:SmtpUsername");
        
        if (string.IsNullOrWhiteSpace(_smtpPassword))
            missingConfig.Add("Email:SmtpPassword");

        if (missingConfig.Any())
        {
            var message = $"SMTP configuration is incomplete. Missing: {string.Join(", ", missingConfig)}. " +
                         "Emails will not be sent. Configure these settings in appsettings.json or environment variables.";

            if (failFast)
            {
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }
            else
            {
                _logger.LogWarning(message);
            }
        }
        else
        {
            _logger.LogInformation("SMTP configuration validated successfully. Email service is ready.");
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
            var displayName = userName ?? email;
            var inviter = inviterName ?? "A team member";
            var project = projectName ?? "the project";
            var roleName = role ?? "Member";
            var expiration = expirationDate?.ToString("MMMM dd, yyyy 'at' HH:mm") ?? "30 days";

            var subject = $"Invitation to join {project} on IntelliPM";
            var htmlBody = GetInvitationEmailTemplate(displayName, inviter, project, roleName, invitationLink, expiration);

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation(
                "Invitation email sent successfully to {Email} for project {ProjectName}",
                email, project);
        }
        catch (EmailServiceException)
        {
            // Re-throw EmailServiceException to allow callers to handle gracefully
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to send invitation email to {Email}. Host: {Host}, Port: {Port}, FromEmail: {FromEmail}. Error: {ErrorMessage}",
                email, _smtpHost, _smtpPort, _fromEmail, ex.Message);
            
            // Wrap in EmailServiceException for consistent error handling
            throw new EmailServiceException(
                $"Failed to send invitation email to {email}. Please check SMTP configuration.",
                ex);
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
            var displayName = userName ?? email;
            var expiration = expirationTime ?? "1 hour";

            var subject = "IntelliPM - Password Reset Request";
            var htmlBody = GetPasswordResetEmailTemplate(displayName, resetLink, expiration);

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation("Password reset email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            // Graceful degradation - don't throw
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
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3001";
            var login = loginLink ?? $"{frontendBaseUrl.TrimEnd('/')}/login";

            var subject = "Welcome to IntelliPM!";
            var htmlBody = GetWelcomeEmailTemplate(username, login);

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation("Welcome email sent successfully to {Email} for user {Username}", email, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            // Graceful degradation - don't throw
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
            var subject = $"Invitation à rejoindre {organizationName} sur IntelliPM";
            var htmlBody = GetOrganizationInvitationEmailTemplate(
                recipientFirstName, recipientLastName, organizationName, inviterName, role, invitationLink);

            await SendEmailAsync(recipientEmail, subject, htmlBody, ct);
            
        _logger.LogInformation(
                "Organization invitation email sent successfully to {RecipientEmail} for {RecipientFirstName} {RecipientLastName} " +
                "with role {Role} in organization {OrganizationName}",
                recipientEmail, recipientFirstName, recipientLastName, role, organizationName);
            
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
            var htmlBody = GetTestEmailTemplate();

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation("Test email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email to {Email}", email);
            // Graceful degradation - don't throw
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
            var htmlBody = GetAIQuotaUpdatedEmailTemplate(organizationName, oldTier, newTier, tierChangeText);

            await SendEmailAsync(recipientEmail, subject, htmlBody, ct);
            
        _logger.LogInformation(
                "AI quota updated email sent successfully to {RecipientEmail} for organization {OrganizationName} " +
                "(Old Tier: {OldTier}, New Tier: {NewTier})",
                recipientEmail, organizationName, oldTier, newTier);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send AI quota updated email to {RecipientEmail}", recipientEmail);
            return false;
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
            var htmlBody = GetAIDisabledNotificationEmailTemplate(organizationName, reason, isPermanent, modeText);

            await SendEmailAsync(recipientEmail, subject, htmlBody, ct);

        _logger.LogWarning(
                "AI disabled notification email sent successfully to {RecipientEmail} for organization {OrganizationName} " +
                "(Reason: {Reason}, Permanent: {IsPermanent})",
                recipientEmail, organizationName, reason, isPermanent);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send AI disabled notification email to {RecipientEmail}", recipientEmail);
            return false;
        }
    }

    public async Task SendMentionNotificationEmailAsync(
        string email,
        string mentionedBy,
        string entityType,
        string entityTitle,
        string commentPreview,
        string entityUrl,
        CancellationToken ct = default)
    {
        try
        {
            var subject = $"{mentionedBy} mentioned you in {entityType}: {entityTitle}";
            var htmlBody = GetMentionNotificationEmailTemplate(mentionedBy, entityType, entityTitle, commentPreview, entityUrl);

            await SendEmailAsync(email, subject, htmlBody, ct);
            
            _logger.LogInformation(
                "Mention notification email sent successfully to {Email} for mention by {MentionedBy} in {EntityType} {EntityTitle}",
                email, mentionedBy, entityType, entityTitle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send mention notification email to {Email}", email);
            // Graceful degradation - don't throw
        }
    }

    /// <summary>
    /// Private method to send email via SMTP with proper error handling.
    /// Throws EmailServiceException if SMTP is not configured or if sending fails.
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        // Check if SMTP is configured
        if (string.IsNullOrEmpty(_smtpHost) || string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
        {
            var errorMessage = $"SMTP not configured. Cannot send email to {toEmail} with subject: {subject}. " +
                              "Please configure Email:SmtpHost, Email:SmtpUsername, and Email:SmtpPassword in appsettings.json";
            
            _logger.LogError(errorMessage);
            throw new EmailServiceException(errorMessage);
        }

        try
        {
            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _enableSsl,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                Timeout = _timeoutSeconds * 1000 // Convert to milliseconds
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            await smtpClient.SendMailAsync(mailMessage, ct);
            
            _logger.LogDebug("Email sent successfully to {ToEmail} with subject: {Subject}", toEmail, subject);
        }
        catch (SmtpException smtpEx)
        {
            var errorMessage = $"SMTP error sending email to {toEmail}. Host: {_smtpHost}, Port: {_smtpPort}, FromEmail: {_fromEmail}. " +
                              $"SMTP Error: {smtpEx.Message}";
            
            _logger.LogError(smtpEx,
                "SMTP error sending email to {ToEmail}. Host: {Host}, Port: {Port}, Username: {Username}, FromEmail: {FromEmail}. Error: {ErrorMessage}",
                toEmail, _smtpHost, _smtpPort, _smtpUsername, _fromEmail, smtpEx.Message);
            
            throw new EmailServiceException(errorMessage, smtpEx);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to send email to {toEmail}. Host: {_smtpHost}, Port: {_smtpPort}, FromEmail: {_fromEmail}. " +
                              $"Error: {ex.Message}";
            
            _logger.LogError(ex,
                "Failed to send email via SMTP to {ToEmail}. Host: {Host}, Port: {Port}, Username: {Username}, FromEmail: {FromEmail}. Error: {ErrorMessage}",
                toEmail, _smtpHost, _smtpPort, _smtpUsername, _fromEmail, ex.Message);
            
            throw new EmailServiceException(errorMessage, ex);
        }
    }

    #region Email Templates

    private static string GetInvitationEmailTemplate(string userName, string inviterName, string projectName, string role, string invitationLink, string expirationDate)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Invitation to Join IntelliPM</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f3f4f6;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px; text-align: center; background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">You're Invited!</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                Hello <strong>{userName}</strong>,
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                <strong>{inviterName}</strong> has invited you to join <strong>{projectName}</strong> on IntelliPM as a <strong>{role}</strong>.
                            </p>
                            <div style=""text-align: center; margin: 40px 0;"">
                                <a href=""{invitationLink}"" style=""display: inline-block; padding: 14px 32px; background-color: #2563eb; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">Accept Invitation</a>
                            </div>
                            <p style=""margin: 20px 0 0 0; color: #6b7280; font-size: 14px; line-height: 1.6;"">
                                This invitation will expire on <strong>{expirationDate}</strong>. If you have any questions, please contact {inviterName}.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #6b7280; font-size: 12px; text-align: center;"">
                                This is an automated invitation email from IntelliPM. If you did not expect this invitation, you can safely ignore this email.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetPasswordResetEmailTemplate(string userName, string resetLink, string expirationTime)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset Request</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f3f4f6;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px; text-align: center; background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">Password Reset Request</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                Hello <strong>{userName}</strong>,
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                We received a request to reset your password for your IntelliPM account. Click the button below to create a new password:
                            </p>
                            <div style=""text-align: center; margin: 40px 0;"">
                                <a href=""{resetLink}"" style=""display: inline-block; padding: 14px 32px; background-color: #dc2626; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">Reset Password</a>
                            </div>
                            <p style=""margin: 20px 0 0 0; color: #6b7280; font-size: 14px; line-height: 1.6;"">
                                This link will expire in <strong>{expirationTime}</strong>. If you did not request a password reset, please ignore this email or contact support if you have concerns.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #6b7280; font-size: 12px; text-align: center;"">
                                This is an automated password reset email from IntelliPM. For security reasons, never share this link with anyone.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetWelcomeEmailTemplate(string userName, string loginLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to IntelliPM</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f3f4f6;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px; text-align: center; background: linear-gradient(135deg, #10b981 0%, #059669 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">Welcome to IntelliPM!</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                Hello <strong>{userName}</strong>,
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                Welcome to IntelliPM! Your account has been successfully created. We're excited to have you on board.
                            </p>
                            <div style=""text-align: center; margin: 40px 0;"">
                                <a href=""{loginLink}"" style=""display: inline-block; padding: 14px 32px; background-color: #10b981; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">Get Started</a>
                            </div>
                            <p style=""margin: 20px 0 0 0; color: #6b7280; font-size: 14px; line-height: 1.6;"">
                                If you have any questions or need help getting started, don't hesitate to reach out to our support team.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #6b7280; font-size: 12px; text-align: center;"">
                                This is an automated welcome email from IntelliPM.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetOrganizationInvitationEmailTemplate(string firstName, string lastName, string organizationName, string inviterName, string role, string invitationLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""fr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Invitation à rejoindre {organizationName}</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f3f4f6;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px; text-align: center; background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">Vous êtes invité(e) !</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                Bonjour <strong>{firstName} {lastName}</strong>,
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                <strong>{inviterName}</strong> vous a invité(e) à rejoindre l'organisation <strong>{organizationName}</strong> sur IntelliPM en tant que <strong>{role}</strong>.
                            </p>
                            <div style=""text-align: center; margin: 40px 0;"">
                                <a href=""{invitationLink}"" style=""display: inline-block; padding: 14px 32px; background-color: #2563eb; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">Accepter l'invitation</a>
                            </div>
                            <p style=""margin: 20px 0 0 0; color: #6b7280; font-size: 14px; line-height: 1.6;"">
                                Si vous avez des questions, n'hésitez pas à contacter {inviterName}.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #6b7280; font-size: 12px; text-align: center;"">
                                Ceci est un email d'invitation automatique d'IntelliPM. Si vous n'avez pas demandé cette invitation, vous pouvez ignorer cet email en toute sécurité.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetTestEmailTemplate()
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Test Email</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f3f4f6;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px; text-align: center; background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">Test Email from IntelliPM</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                This is a test email to verify your SMTP configuration.
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                If you received this email, your email settings are configured correctly!
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #6b7280; font-size: 12px; text-align: center;"">
                                This is an automated test email sent from the IntelliPM administration panel.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetAIQuotaUpdatedEmailTemplate(string organizationName, string oldTier, string newTier, string tierChangeText)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>AI Quota Updated</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f3f4f6;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px; text-align: center; background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">AI Quota Updated</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                Hello,
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                {tierChangeText}
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                This change affects the AI usage limits for your organization. You can view your current quota status in the IntelliPM dashboard.
                            </p>
                            <div style=""background-color: #f3f4f6; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                                <p style=""margin: 0; color: #374151; font-size: 14px;""><strong>Organization:</strong> {organizationName}</p>
                                <p style=""margin: 5px 0 0 0; color: #374151; font-size: 14px;""><strong>Previous Tier:</strong> {oldTier}</p>
                                <p style=""margin: 5px 0 0 0; color: #374151; font-size: 14px;""><strong>New Tier:</strong> {newTier}</p>
                            </div>
                            <p style=""margin: 20px 0 0 0; color: #6b7280; font-size: 14px; line-height: 1.6;"">
                                If you have any questions about this change, please contact your system administrator.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #6b7280; font-size: 12px; text-align: center;"">
                                This is an automated notification from IntelliPM.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetAIDisabledNotificationEmailTemplate(string organizationName, string reason, bool isPermanent, string modeText)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>AI Features Disabled</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f3f4f6;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px; text-align: center; background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">AI Features Disabled</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                Hello,
                            </p>
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                This is an important notification regarding your organization's AI features on IntelliPM.
                            </p>
                            <div style=""background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 15px; margin: 20px 0;"">
                                <p style=""margin: 0; color: #991b1b; font-size: 14px; font-weight: 600;"">AI features have been {modeText} disabled for your organization.</p>
                            </div>
                            <div style=""background-color: #f3f4f6; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                                <p style=""margin: 0; color: #374151; font-size: 14px;""><strong>Organization:</strong> {organizationName}</p>
                                <p style=""margin: 5px 0 0 0; color: #374151; font-size: 14px;""><strong>Status:</strong> {modeText} Disabled</p>
                                <p style=""margin: 5px 0 0 0; color: #374151; font-size: 14px;""><strong>Reason:</strong> {reason}</p>
                            </div>
                            <p style=""margin: 20px 0 10px 0; color: #374151; font-size: 16px; font-weight: 600;"">What this means:</p>
                            <ul style=""margin: 0 0 20px 0; padding-left: 20px; color: #374151; font-size: 14px; line-height: 1.8;"">
                                <li>All AI agent operations are currently blocked</li>
                                <li>AI decision-making features are unavailable</li>
                                <li>Historical AI data and decisions are preserved</li>
                                <li>You can still access all other IntelliPM features</li>
                            </ul>
                            <p style=""margin: 20px 0 0 0; color: #6b7280; font-size: 14px; line-height: 1.6;"">
                                If you have any questions or concerns about this action, please contact your system administrator or IntelliPM support immediately.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #6b7280; font-size: 12px; text-align: center;"">
                                This is an automated critical notification from IntelliPM.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GetMentionNotificationEmailTemplate(string mentionedBy, string entityType, string entityTitle, string commentPreview, string entityUrl)
    {
        // Truncate comment preview if too long
        var preview = commentPreview.Length > 200 
            ? commentPreview.Substring(0, 200) + "..." 
            : commentPreview;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>You were mentioned</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f3f4f6;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px; text-align: center; background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;"">You were mentioned</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px; line-height: 1.6;"">
                                <strong>{mentionedBy}</strong> mentioned you in a comment on <strong>{entityType}: {entityTitle}</strong>.
                            </p>
                            <div style=""background-color: #f9fafb; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; border-radius: 4px;"">
                                <p style=""margin: 0 0 10px 0; color: #6b7280; font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;"">Comment Preview:</p>
                                <p style=""margin: 0; color: #374151; font-size: 14px; line-height: 1.6; font-style: italic;"">""{preview}""</p>
                            </div>
                            <div style=""text-align: center; margin: 40px 0;"">
                                <a href=""{entityUrl}"" style=""display: inline-block; padding: 14px 32px; background-color: #f59e0b; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">View {entityType}</a>
                            </div>
                            <p style=""margin: 20px 0 0 0; color: #6b7280; font-size: 14px; line-height: 1.6;"">
                                Click the button above to view the {entityType.ToLower()} and respond to the comment.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; color: #6b7280; font-size: 12px; text-align: center;"">
                                This is an automated notification from IntelliPM. You can manage your notification preferences in your account settings.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    #endregion
}
