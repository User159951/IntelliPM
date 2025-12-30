using MediatR;
using IntelliPM.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Settings.Commands;

public class SendTestEmailCommandHandler : IRequestHandler<SendTestEmailCommand, SendTestEmailResponse>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendTestEmailCommandHandler> _logger;

    public SendTestEmailCommandHandler(
        IEmailService emailService,
        ILogger<SendTestEmailCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SendTestEmailResponse> Handle(SendTestEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _emailService.SendTestEmailAsync(request.Email, cancellationToken);

            _logger.LogInformation("Test email sent successfully to {Email}", request.Email);
            
            return new SendTestEmailResponse(
                true,
                $"Test email sent successfully to {request.Email}. Please check your inbox.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email to {Email}", request.Email);
            return new SendTestEmailResponse(
                false,
                $"Failed to send test email: {ex.Message}");
        }
    }
}

