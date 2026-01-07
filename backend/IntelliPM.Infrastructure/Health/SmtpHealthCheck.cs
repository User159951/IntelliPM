using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using IntelliPM.Infrastructure.Services;

namespace IntelliPM.Infrastructure.Health;

/// <summary>
/// Health check for SMTP email service connectivity
/// Tests SMTP connection and authentication without sending an email
/// </summary>
public class SmtpHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpHealthCheck> _logger;

    public SmtpHealthCheck(IConfiguration configuration, ILogger<SmtpHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailSection = _configuration.GetSection("Email");
            var smtpHost = emailSection["SmtpHost"];
            var smtpPort = emailSection.GetValue<int>("SmtpPort", 587);
            var smtpUsername = emailSection["SmtpUsername"];
            var smtpPassword = emailSection["SmtpPassword"];
            var secureSocketOptionsOverride = emailSection["SecureSocketOptions"];

            // Check if SMTP is configured
            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                return HealthCheckResult.Degraded("SMTP not configured. Email functionality will be unavailable.");
            }

            if (string.IsNullOrWhiteSpace(smtpUsername) || string.IsNullOrWhiteSpace(smtpPassword))
            {
                return HealthCheckResult.Degraded("SMTP credentials not configured. Email functionality will be unavailable.");
            }

            // Determine socket options using the same logic as SmtpEmailService
            var socketOptions = SmtpSocketOptionsHelper.GetSecureSocketOptions(
                smtpPort,
                secureSocketOptionsOverride,
                _logger);

            // Test SMTP connection with a short timeout for health checks
            using var client = new SmtpClient();
            client.CheckCertificateRevocation = false; // Match SmtpEmailService behavior
            client.Timeout = 5000; // 5 seconds timeout for health checks

            try
            {
                await client.ConnectAsync(smtpHost, smtpPort, socketOptions, cancellationToken);
                
                // Test authentication
                await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
                
                await client.DisconnectAsync(true, cancellationToken);

                return HealthCheckResult.Healthy($"SMTP connection and authentication successful. Host: {smtpHost}, Port: {smtpPort}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMTP health check failed for {Host}:{Port}", smtpHost, smtpPort);
                
                // Return Degraded instead of Unhealthy - email is optional for API functionality
                return HealthCheckResult.Degraded($"SMTP connection failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SMTP health check");
            return HealthCheckResult.Degraded($"SMTP health check error: {ex.Message}");
        }
    }
}

