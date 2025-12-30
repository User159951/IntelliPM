using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace IntelliPM.Application.Identity.Commands;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, RequestPasswordResetResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

    public RequestPasswordResetCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<RequestPasswordResetCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<RequestPasswordResetResponse> Handle(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email or username
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Email == request.EmailOrUsername || u.Username == request.EmailOrUsername, cancellationToken);

        // Always return success to prevent email enumeration attacks
        // If user doesn't exist, we still return success but don't send email
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Password reset requested for non-existent or inactive user: {EmailOrUsername}", request.EmailOrUsername);
            return new RequestPasswordResetResponse(
                Success: true,
                Message: "If an account exists with that email or username, a password reset link has been sent."
            );
        }

        // Generate secure token (32 bytes, Base64URL encoded)
        var tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        var token = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        // Create password reset token (expires in 1 hour)
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            IsUsed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Invalidate any existing unused tokens for this user
        var existingTokensRepo = _unitOfWork.Repository<PasswordResetToken>();
        var existingTokens = await existingTokensRepo.Query()
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var existingToken in existingTokens)
        {
            existingToken.IsUsed = true; // Mark as used to invalidate
            existingTokensRepo.Update(existingToken);
        }

        await existingTokensRepo.AddAsync(resetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Build reset link
        var frontendUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3001";
        var resetLink = $"{frontendUrl}/reset-password/{token}";

        // Send password reset email
        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                email: user.Email,
                resetToken: token,
                resetLink: resetLink,
                userName: user.Username,
                expirationTime: "1 hour",
                ct: cancellationToken);

            _logger.LogInformation("Password reset email sent to {Email} for user {UserId}", user.Email, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            // Still return success to prevent information disclosure
        }

        return new RequestPasswordResetResponse(
            Success: true,
            Message: "If an account exists with that email or username, a password reset link has been sent."
        );
    }
}

