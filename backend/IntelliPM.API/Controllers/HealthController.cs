using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Services;
using IntelliPM.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IEmailService? _emailService;

    public HealthController(
        AppDbContext context,
        ILogger<HealthController> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IEmailService? emailService = null)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
        _emailService = emailService;
    }

    /// <summary>
    /// Get API health status
    /// </summary>
    /// <remarks>
    /// Checks database connectivity and returns overall API health status.
    /// Returns 200 OK if healthy, 503 Service Unavailable if unhealthy.
    /// </remarks>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Health status with checks</returns>
    /// <response code="200">API is healthy</response>
    /// <response code="503">API is unhealthy (database connection failed)</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        try
        {
            // Check database connectivity
            await _context.Database.CanConnectAsync(ct);

            var health = new
            {
                status = "Healthy",
                timestamp = DateTimeOffset.UtcNow,
                checks = new
                {
                    database = "Healthy",
                    api = "Healthy"
                }
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            
            var health = new
            {
                status = "Unhealthy",
                timestamp = DateTimeOffset.UtcNow,
                checks = new
                {
                    database = "Unhealthy",
                    api = "Healthy"
                },
                error = ex.Message
            };

            return StatusCode(503, health);
        }
    }

#if DEBUG
    /// <summary>
    /// ⚠️ WARNING: DEBUG-ONLY ENDPOINT ⚠️
    /// 
    /// DEV-ONLY: SMTP connection diagnostics endpoint.
    /// Tests SMTP connectivity without sending an email.
    /// 
    /// This endpoint is ONLY available in DEBUG builds and will be COMPLETELY DISABLED in Release builds.
    /// </summary>
    [HttpGet("smtp")]
    public async Task<IActionResult> TestSmtpConnection(CancellationToken ct)
    {
        // Only allow in Development environment
        if (!_environment.IsDevelopment())
        {
            _logger.LogWarning("SMTP diagnostics endpoint accessed in non-development environment");
            return NotFound(new { error = "Endpoint not available in this environment" });
        }

        _logger.LogInformation("SMTP diagnostics endpoint called");

        var emailSection = _configuration.GetSection("Email");
        var smtpHost = emailSection["SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = emailSection.GetValue<int>("SmtpPort", 587);
        var smtpUsername = emailSection["SmtpUsername"] ?? string.Empty;
        var smtpPassword = emailSection["SmtpPassword"] ?? string.Empty;
        var secureSocketOptionsOverride = emailSection["SecureSocketOptions"];

        // Determine socket options using the same logic as SmtpEmailService
        var socketOptions = SmtpSocketOptionsHelper.GetSecureSocketOptions(
            smtpPort, 
            secureSocketOptionsOverride, 
            _logger);

        using var client = new SmtpClient();
        try
        {
            _logger.LogInformation("Attempting SMTP connection to {Host}:{Port} with socket options {Options}", 
                smtpHost, smtpPort, socketOptions);

            // Enable certificate revocation checking for security
            client.CheckCertificateRevocation = true;

            // Attempt connection
            await client.ConnectAsync(smtpHost, smtpPort, socketOptions, ct);
            _logger.LogInformation("SMTP connection successful");

            // Attempt authentication if credentials are provided
            bool authenticated = false;
            if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
            {
                try
                {
                    await client.AuthenticateAsync(smtpUsername, smtpPassword, ct);
                    authenticated = true;
                    _logger.LogInformation("SMTP authentication successful");
                }
                catch (Exception authEx)
                {
                    _logger.LogWarning(authEx, "SMTP authentication failed");
                    // Continue to return connection success but auth failure
                }
            }
            else
            {
                _logger.LogInformation("SMTP credentials not provided, skipping authentication");
            }

            // Disconnect cleanly
            await client.DisconnectAsync(true, ct);

            return Ok(new
            {
                ok = true,
                host = smtpHost,
                port = smtpPort,
                socketOptionsUsed = socketOptions.ToString(),
                authenticated = authenticated,
                message = authenticated 
                    ? "SMTP connection and authentication successful" 
                    : "SMTP connection successful but authentication not attempted or failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP diagnostics failed for {Host}:{Port}", smtpHost, smtpPort);

            // Determine error type without exposing sensitive information
            string errorType = ex.GetType().Name;
            string message = ex.Message;

            // Sanitize error message to avoid exposing secrets
            if (message.Contains(smtpPassword, StringComparison.OrdinalIgnoreCase) && 
                !string.IsNullOrEmpty(smtpPassword))
            {
                message = message.Replace(smtpPassword, "***", StringComparison.OrdinalIgnoreCase);
            }
            if (message.Contains(smtpUsername, StringComparison.OrdinalIgnoreCase) && 
                !string.IsNullOrEmpty(smtpUsername))
            {
                // Only mask username if it looks like a password (contains special chars)
                // Otherwise, username in error messages is usually safe
                if (smtpUsername.Contains("@"))
                {
                    // Keep email addresses visible as they're not secrets
                }
            }

            return StatusCode(500, new
            {
                ok = false,
                errorType = errorType,
                message = message,
                host = smtpHost,
                port = smtpPort,
                socketOptionsUsed = socketOptions.ToString()
            });
        }
    }

    /// <summary>
    /// ⚠️ WARNING: DEBUG-ONLY ENDPOINT ⚠️
    /// 
    /// DEV-ONLY: Send a test email via SMTP.
    /// Uses IEmailService to send a welcome email to the specified address.
    /// 
    /// This endpoint is ONLY available in DEBUG builds and will be COMPLETELY DISABLED in Release builds.
    /// </summary>
    [HttpPost("smtp/send-test")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request, CancellationToken ct)
    {
        // Only allow in Development environment
        if (!_environment.IsDevelopment())
        {
            _logger.LogWarning("SMTP send-test endpoint accessed in non-development environment");
            return NotFound(new { error = "Endpoint not available in this environment" });
        }

        if (_emailService == null)
        {
            return StatusCode(500, new
            {
                ok = false,
                errorType = "ServiceNotRegistered",
                message = "IEmailService is not registered. Ensure Email:Provider is set to 'SMTP' and Email:SmtpUsername is configured."
            });
        }

        if (string.IsNullOrWhiteSpace(request.To))
        {
            return BadRequest(new
            {
                ok = false,
                errorType = "ValidationError",
                message = "Email address is required in 'to' field"
            });
        }

        _logger.LogInformation("SMTP send-test endpoint called for {ToEmail}", request.To);

        try
        {
            // Send a welcome email as a test
            await _emailService.SendWelcomeEmailAsync(
                request.To,
                "Test User",
                null,
                ct);

            _logger.LogInformation("Test email sent successfully to {ToEmail}", request.To);

            return Ok(new
            {
                ok = true,
                message = $"Test email sent successfully to {request.To}",
                to = request.To
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email to {ToEmail}", request.To);

            // Sanitize error message
            string errorType = ex.GetType().Name;
            string message = ex.Message;

            // Remove any potential secrets from error message
            var emailSection = _configuration.GetSection("Email");
            var smtpPassword = emailSection["SmtpPassword"] ?? string.Empty;
            if (!string.IsNullOrEmpty(smtpPassword) && message.Contains(smtpPassword, StringComparison.OrdinalIgnoreCase))
            {
                message = message.Replace(smtpPassword, "***", StringComparison.OrdinalIgnoreCase);
            }

            return StatusCode(500, new
            {
                ok = false,
                errorType = errorType,
                message = message
            });
        }
    }

    /// <summary>
    /// Request model for send-test email endpoint
    /// </summary>
    public class SendTestEmailRequest
    {
        public string To { get; set; } = string.Empty;
    }
#endif

}

