using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Identity.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<ResetPasswordResponse> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Validate passwords match
        if (request.NewPassword != request.ConfirmPassword)
        {
            return new ResetPasswordResponse(
                Success: false,
                Message: "Passwords do not match."
            );
        }

        // Find token
        var resetTokenRepo = _unitOfWork.Repository<PasswordResetToken>();
        var resetToken = await resetTokenRepo.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (resetToken == null)
        {
            _logger.LogWarning("Invalid password reset token used: {Token}", request.Token);
            return new ResetPasswordResponse(
                Success: false,
                Message: "Invalid or expired reset token."
            );
        }

        // Validate token is not expired
        if (resetToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Expired password reset token used: {Token}", request.Token);
            return new ResetPasswordResponse(
                Success: false,
                Message: "Invalid or expired reset token."
            );
        }

        // Validate token is not already used
        if (resetToken.IsUsed)
        {
            _logger.LogWarning("Already used password reset token attempted: {Token}", request.Token);
            return new ResetPasswordResponse(
                Success: false,
                Message: "This reset token has already been used."
            );
        }

        // Get user
        var user = resetToken.User;
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Password reset attempted for inactive user: {UserId}", resetToken.UserId);
            return new ResetPasswordResponse(
                Success: false,
                Message: "User account is inactive."
            );
        }

        // Hash new password
        var (passwordHash, passwordSalt) = _passwordHasher.HashPassword(request.NewPassword);

        // Update user password
        var userRepo = _unitOfWork.Repository<User>();
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        userRepo.Update(user);

        // Mark token as used
        resetToken.IsUsed = true;
        resetTokenRepo.Update(resetToken);

        // Revoke all refresh tokens for security (force re-login)
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        var refreshTokens = await refreshTokenRepo.Query()
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var rt in refreshTokens)
        {
            rt.IsRevoked = true;
            refreshTokenRepo.Update(rt);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successful for user {UserId} (Email: {Email})", user.Id, user.Email);

        return new ResetPasswordResponse(
            Success: true,
            Message: "Password has been reset successfully. Please log in with your new password."
        );
    }
}

