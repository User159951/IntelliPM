using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Services;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for ResetUserAIQuotaOverrideCommand.
/// Deletes a user's AI quota override, reverting to organization default.
/// </summary>
public class ResetUserAIQuotaOverrideCommandHandler : IRequestHandler<ResetUserAIQuotaOverrideCommand, ResetUserAIQuotaOverrideResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly ILogger<ResetUserAIQuotaOverrideCommandHandler> _logger;

    public ResetUserAIQuotaOverrideCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        ILogger<ResetUserAIQuotaOverrideCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _logger = logger;
    }

    public async Task<ResetUserAIQuotaOverrideResponse> Handle(ResetUserAIQuotaOverrideCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can reset user AI quota overrides");
        }

        var currentUserId = _currentUserService.GetUserId();

        if (currentUserId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        // Verify user exists and check organization access
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user == null)
        {
            throw new NotFoundException($"User {request.UserId} not found");
        }

        // Ensure organization access (SuperAdmin can access any, Admin only their own)
        _scopingService.EnsureOrganizationAccess(user.OrganizationId);

        var organizationId = user.OrganizationId;

        // Get organization's active quota for period
        var orgQuota = await _unitOfWork.Repository<AIQuota>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.OrganizationId == organizationId && q.IsActive, ct);

        if (orgQuota == null)
        {
            throw new NotFoundException("No active AI quota found for your organization");
        }

        var periodStart = orgQuota.PeriodStartDate;
        var periodEnd = orgQuota.PeriodEndDate;

        // Find and delete override
        var overrideRepo = _unitOfWork.Repository<UserAIQuotaOverride>();
        var existingOverride = await overrideRepo
            .Query()
            .FirstOrDefaultAsync(o => o.UserId == request.UserId &&
                                     o.PeriodStartDate == periodStart &&
                                     o.PeriodEndDate == periodEnd, ct);

        if (existingOverride == null)
        {
            return new ResetUserAIQuotaOverrideResponse(
                request.UserId,
                true,
                "No override exists for this user"
            );
        }

        // Delete using repository Delete method
        overrideRepo.Delete(existingOverride);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User AI quota override reset for user {UserId} by admin {AdminUserId}",
            request.UserId, currentUserId);

        return new ResetUserAIQuotaOverrideResponse(
            request.UserId,
            true,
            "Quota override reset successfully"
        );
    }
}

