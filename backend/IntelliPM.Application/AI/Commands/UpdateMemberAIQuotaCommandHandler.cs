using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Application.Common.Services;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ApplicationException = IntelliPM.Application.Common.Exceptions.ApplicationException;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for UpdateMemberAIQuotaCommand.
/// Updates or creates user AI quota override (Admin only - own organization).
/// Validates that override values don't exceed organization limits.
/// </summary>
public class UpdateMemberAIQuotaCommandHandler : IRequestHandler<UpdateMemberAIQuotaCommand, MemberAIQuotaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly ILogger<UpdateMemberAIQuotaCommandHandler> _logger;

    public UpdateMemberAIQuotaCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        ILogger<UpdateMemberAIQuotaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _logger = logger;
    }

    public async Task<MemberAIQuotaDto> Handle(UpdateMemberAIQuotaCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can update member AI quotas");
        }

        var currentUserId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (currentUserId == 0 || organizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        // Get target user and ensure they're in the same organization
        var targetUser = await _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (targetUser == null)
        {
            throw new NotFoundException($"User {request.UserId} not found");
        }

        // Ensure organization access (Admin can only modify users in their own organization)
        _scopingService.EnsureOrganizationAccess(targetUser.OrganizationId);

        // Get organization AI quota (base limits for validation)
        var orgQuota = await _unitOfWork.Repository<OrganizationAIQuota>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.OrganizationId == organizationId, ct);

        // Get default limits if org quota doesn't exist
        var defaultLimits = Domain.Constants.AIQuotaConstants.DefaultLimits[Domain.Constants.AIQuotaConstants.Tiers.Free];
        var orgTokenLimit = orgQuota?.MonthlyTokenLimit ?? defaultLimits.MaxTokensPerPeriod;
        var orgRequestLimit = orgQuota?.MonthlyRequestLimit ?? (int?)defaultLimits.MaxRequestsPerPeriod; // Ensure nullable
        var orgIsAIEnabled = orgQuota?.IsAIEnabled ?? true;

        // Validate that override values don't exceed organization limits
        if (request.MonthlyTokenLimitOverride.HasValue && request.MonthlyTokenLimitOverride.Value > orgTokenLimit)
        {
            throw new ApplicationException(
                $"Monthly token limit override ({request.MonthlyTokenLimitOverride.Value}) cannot exceed organization limit ({orgTokenLimit})");
        }

        if (request.MonthlyRequestLimitOverride.HasValue)
        {
            if (orgRequestLimit.HasValue && request.MonthlyRequestLimitOverride.Value > orgRequestLimit.Value)
            {
                throw new ApplicationException(
                    $"Monthly request limit override ({request.MonthlyRequestLimitOverride.Value}) cannot exceed organization limit ({orgRequestLimit.Value})");
            }
        }

        // If org kill-switch is disabled, only allow enabling via override (not disabling further)
        if (!orgIsAIEnabled && request.IsAIEnabledOverride.HasValue && !request.IsAIEnabledOverride.Value)
        {
            throw new ApplicationException(
                "Cannot disable AI for a user when organization AI is already disabled. Set IsAIEnabledOverride to true to enable for this user.");
        }

        // Find existing user quota or create new one
        var userQuotaRepo = _unitOfWork.Repository<UserAIQuota>();
        var existingQuota = await userQuotaRepo
            .Query()
            .FirstOrDefaultAsync(q => q.UserId == request.UserId && q.OrganizationId == organizationId, ct);

        UserAIQuota userQuota;

        if (existingQuota != null)
        {
            // Update existing override
            if (request.MonthlyTokenLimitOverride.HasValue)
                existingQuota.MonthlyTokenLimitOverride = request.MonthlyTokenLimitOverride;
            else if (request.MonthlyTokenLimitOverride == null && request.MonthlyTokenLimitOverride == null)
            {
                // Explicit null means remove override (use org default)
                existingQuota.MonthlyTokenLimitOverride = null;
            }

            if (request.MonthlyRequestLimitOverride.HasValue)
                existingQuota.MonthlyRequestLimitOverride = request.MonthlyRequestLimitOverride;
            else if (request.MonthlyRequestLimitOverride == null)
            {
                existingQuota.MonthlyRequestLimitOverride = null;
            }

            if (request.IsAIEnabledOverride.HasValue)
                existingQuota.IsAIEnabledOverride = request.IsAIEnabledOverride;
            else if (request.IsAIEnabledOverride == null)
            {
                existingQuota.IsAIEnabledOverride = null;
            }

            existingQuota.UpdatedAt = DateTimeOffset.UtcNow;
            userQuotaRepo.Update(existingQuota);
            userQuota = existingQuota;
        }
        else
        {
            // Create new override
            userQuota = new UserAIQuota
            {
                UserId = request.UserId,
                OrganizationId = organizationId,
                MonthlyTokenLimitOverride = request.MonthlyTokenLimitOverride,
                MonthlyRequestLimitOverride = request.MonthlyRequestLimitOverride,
                IsAIEnabledOverride = request.IsAIEnabledOverride,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null
            };

            await userQuotaRepo.AddAsync(userQuota, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User AI quota override updated for user {UserId} in organization {OrganizationId} by admin {AdminUserId}",
            request.UserId, organizationId, currentUserId);

        // Build response DTO with effective quota
        var orgQuotaBase = new OrganizationQuotaBaseDto(
            orgTokenLimit,
            orgRequestLimit,
            orgIsAIEnabled);

        var effectiveTokenLimit = userQuota.MonthlyTokenLimitOverride ?? orgQuotaBase.MonthlyTokenLimit;
        var effectiveRequestLimit = userQuota.MonthlyRequestLimitOverride ?? orgQuotaBase.MonthlyRequestLimit;
        var effectiveIsAIEnabled = orgQuotaBase.IsAIEnabled
            ? (userQuota.IsAIEnabledOverride ?? true)
            : (userQuota.IsAIEnabledOverride ?? false);

        var effectiveQuota = new EffectiveMemberQuotaDto(
            effectiveTokenLimit,
            effectiveRequestLimit,
            effectiveIsAIEnabled,
            userQuota.MonthlyTokenLimitOverride.HasValue ||
            userQuota.MonthlyRequestLimitOverride.HasValue ||
            userQuota.IsAIEnabledOverride.HasValue
        );

        var overrideDto = new UserQuotaOverrideDto(
            userQuota.MonthlyTokenLimitOverride,
            userQuota.MonthlyRequestLimitOverride,
            userQuota.IsAIEnabledOverride,
            userQuota.CreatedAt,
            userQuota.UpdatedAt
        );

        return new MemberAIQuotaDto(
            targetUser.Id,
            targetUser.Email,
            targetUser.FirstName,
            targetUser.LastName,
            $"{targetUser.FirstName} {targetUser.LastName}",
            targetUser.GlobalRole.ToString(),
            targetUser.OrganizationId,
            targetUser.Organization.Name,
            effectiveQuota,
            overrideDto,
            orgQuotaBase
        );
    }
}

