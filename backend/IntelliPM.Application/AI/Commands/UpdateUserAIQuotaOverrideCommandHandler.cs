using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Services;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for UpdateUserAIQuotaOverrideCommand.
/// Creates or updates a user's AI quota override.
/// </summary>
public class UpdateUserAIQuotaOverrideCommandHandler : IRequestHandler<UpdateUserAIQuotaOverrideCommand, UpdateUserAIQuotaOverrideResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly ILogger<UpdateUserAIQuotaOverrideCommandHandler> _logger;

    public UpdateUserAIQuotaOverrideCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        ILogger<UpdateUserAIQuotaOverrideCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _logger = logger;
    }

    public async Task<UpdateUserAIQuotaOverrideResponse> Handle(UpdateUserAIQuotaOverrideCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can update user AI quota overrides");
        }

        var currentUserId = _currentUserService.GetUserId();

        if (currentUserId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        // Verify user exists and check organization access with error handling
        User? user;
        try
        {
            user = await _unitOfWork.Repository<User>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        }
        catch (Exception userEx)
        {
            _logger.LogError(userEx, "Database error when querying user {UserId}", request.UserId);
            throw new NotFoundException($"User {request.UserId} not found");
        }

        if (user == null)
        {
            throw new NotFoundException($"User {request.UserId} not found");
        }

        // Ensure organization access (SuperAdmin can access any, Admin only their own)
        _scopingService.EnsureOrganizationAccess(user.OrganizationId);

        var organizationId = user.OrganizationId;

        // Get organization's active quota for period with error handling
        AIQuota? orgQuota;
        try
        {
            orgQuota = await _unitOfWork.Repository<AIQuota>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.OrganizationId == organizationId && q.IsActive, ct);
        }
        catch (Exception quotaEx)
        {
            _logger.LogError(quotaEx, "Database error when querying AI quota for organization {OrganizationId}", organizationId);
            throw new NotFoundException("No active AI quota found for your organization");
        }

        if (orgQuota == null)
        {
            throw new NotFoundException("No active AI quota found for your organization");
        }

        var periodStart = orgQuota.PeriodStartDate;
        var periodEnd = orgQuota.PeriodEndDate;

        // Find existing override or create new one with error handling
        var overrideRepo = _unitOfWork.Repository<UserAIQuotaOverride>();
        UserAIQuotaOverride? existingOverride = null;
        try
        {
            existingOverride = await overrideRepo
                .Query()
                .FirstOrDefaultAsync(o => o.UserId == request.UserId &&
                                         o.PeriodStartDate == periodStart &&
                                         o.PeriodEndDate == periodEnd, ct);
        }
        catch (Exception overrideQueryEx)
        {
            _logger.LogError(overrideQueryEx, "Database error when querying existing override for user {UserId}", request.UserId);
            throw new IntelliPM.Application.Common.Exceptions.ApplicationException("Error querying quota override");
        }

        UserAIQuotaOverride overrideEntity;

        if (existingOverride != null)
        {
            // Update existing override
            existingOverride.MaxTokensPerPeriod = request.MaxTokensPerPeriod;
            existingOverride.MaxRequestsPerPeriod = request.MaxRequestsPerPeriod;
            existingOverride.MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod;
            existingOverride.MaxCostPerPeriod = request.MaxCostPerPeriod;
            existingOverride.Reason = request.Reason;
            existingOverride.UpdatedAt = DateTimeOffset.UtcNow;
            existingOverride.CreatedByUserId = currentUserId;

            overrideEntity = existingOverride;
        }
        else
        {
            // Create new override
            overrideEntity = new UserAIQuotaOverride
            {
                OrganizationId = organizationId,
                UserId = request.UserId,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                MaxTokensPerPeriod = request.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = request.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = request.MaxCostPerPeriod,
                Reason = request.Reason,
                CreatedByUserId = currentUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            try
            {
                await overrideRepo.AddAsync(overrideEntity, ct);
            }
            catch (Exception addEx)
            {
                _logger.LogError(addEx, "Error adding quota override for user {UserId}", request.UserId);
                throw new IntelliPM.Application.Common.Exceptions.ApplicationException("Error creating quota override");
            }
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception saveEx)
        {
            _logger.LogError(saveEx, "Error saving quota override for user {UserId}", request.UserId);
            throw new IntelliPM.Application.Common.Exceptions.ApplicationException("Error saving quota override");
        }

        _logger.LogInformation(
            "User AI quota override updated for user {UserId} by admin {AdminUserId}",
            request.UserId, currentUserId);

        // Compute effective quota with error handling
        Domain.Entities.EffectiveQuotaLimits effectiveQuota;
        try
        {
            effectiveQuota = overrideEntity.GetEffectiveLimits(orgQuota);
        }
        catch (Exception effectiveEx)
        {
            _logger.LogWarning(effectiveEx, "Error computing effective quota for user {UserId}. Using default values.", request.UserId);
            // Use default values if GetEffectiveLimits fails
            effectiveQuota = new Domain.Entities.EffectiveQuotaLimits
            {
                MaxTokensPerPeriod = overrideEntity.MaxTokensPerPeriod ?? orgQuota.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = overrideEntity.MaxRequestsPerPeriod ?? orgQuota.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = overrideEntity.MaxDecisionsPerPeriod ?? orgQuota.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = overrideEntity.MaxCostPerPeriod ?? orgQuota.MaxCostPerPeriod,
                HasOverride = existingOverride != null
            };
        }

        return new UpdateUserAIQuotaOverrideResponse(
            overrideEntity.Id,
            overrideEntity.UserId,
            new EffectiveQuotaDto(
                effectiveQuota.MaxTokensPerPeriod,
                effectiveQuota.MaxRequestsPerPeriod,
                effectiveQuota.MaxDecisionsPerPeriod,
                effectiveQuota.MaxCostPerPeriod,
                effectiveQuota.HasOverride
            ),
            new QuotaOverrideDto(
                overrideEntity.MaxTokensPerPeriod,
                overrideEntity.MaxRequestsPerPeriod,
                overrideEntity.MaxDecisionsPerPeriod,
                overrideEntity.MaxCostPerPeriod,
                overrideEntity.CreatedAt,
                overrideEntity.Reason
            )
        );
    }
}

