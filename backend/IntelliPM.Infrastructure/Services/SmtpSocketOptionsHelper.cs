using MailKit.Security;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Helper class for determining SMTP secure socket options.
/// This logic is extracted to enable unit testing without network calls.
/// </summary>
public static class SmtpSocketOptionsHelper
{
    /// <summary>
    /// Determines the secure socket options for SMTP connection.
    /// Uses override if provided and valid, otherwise uses port-based logic.
    /// </summary>
    /// <param name="smtpPort">The SMTP port number.</param>
    /// <param name="secureSocketOptionsOverride">Optional override value from configuration.</param>
    /// <param name="logger">Optional logger for warnings/debug messages.</param>
    /// <returns>The SecureSocketOptions to use for the SMTP connection.</returns>
    public static SecureSocketOptions GetSecureSocketOptions(
        int smtpPort,
        string? secureSocketOptionsOverride = null,
        ILogger? logger = null)
    {
        // If override is provided, try to use it
        if (!string.IsNullOrWhiteSpace(secureSocketOptionsOverride))
        {
            var overrideValue = secureSocketOptionsOverride.Trim();
            if (Enum.TryParse<SecureSocketOptions>(overrideValue, ignoreCase: true, out var parsedOptions))
            {
                // Validate that it's one of the accepted values
                if (parsedOptions == SecureSocketOptions.Auto ||
                    parsedOptions == SecureSocketOptions.None ||
                    parsedOptions == SecureSocketOptions.StartTls ||
                    parsedOptions == SecureSocketOptions.StartTlsWhenAvailable ||
                    parsedOptions == SecureSocketOptions.SslOnConnect)
                {
                    logger?.LogDebug("Using SecureSocketOptions override: {Options}", parsedOptions);
                    return parsedOptions;
                }
            }

            // Invalid value provided, log warning and fall back to port-based logic
            logger?.LogWarning(
                "Invalid Email:SecureSocketOptions value '{Value}'. Accepted values: Auto, None, StartTls, StartTlsWhenAvailable, SslOnConnect. Falling back to port-based logic.",
                overrideValue);
        }

        // Fall back to port-based logic:
        // Port 465 => SslOnConnect (implicit SSL/TLS)
        // Port 587 => StartTls (explicit TLS via STARTTLS command)
        // Otherwise => Auto (let MailKit decide)
        var portBasedOptions = smtpPort switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.Auto
        };

        logger?.LogDebug("Using port-based SecureSocketOptions: {Options} for port {Port}", portBasedOptions, smtpPort);
        return portBasedOptions;
    }
}

