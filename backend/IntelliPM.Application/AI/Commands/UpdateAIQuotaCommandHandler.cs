using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Services;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for updating AI quota limits for organizations.
/// Handles tier changes, audit logging, and email notifications.
/// </summary>
public class UpdateAIQuotaCommandHandler : IRequestHandler<UpdateAIQuotaCommand, UpdateAIQuotaResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly IEmailService _emailService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<UpdateAIQuotaCommandHandler> _logger;

    public UpdateAIQuotaCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        IEmailService emailService,
        ISettingsService settingsService,
        ILogger<UpdateAIQuotaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _emailService = emailService;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<UpdateAIQuotaResponse> Handle(UpdateAIQuotaCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can update AI quotas");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        // Ensure organization access (SuperAdmin can update any, Admin only their own)
        _scopingService.EnsureOrganizationAccess(request.OrganizationId);

        _logger.LogInformation("Updating AI quota for organization {OrganizationId} to tier {TierName}",
            request.OrganizationId, request.TierName);

        // Get current active quota
        var currentQuota = await _unitOfWork.Repository<AIQuota>()
            .Query()
            .FirstOrDefaultAsync(q => q.OrganizationId == request.OrganizationId && q.IsActive, ct);

        var organization = await _unitOfWork.Repository<Organization>()
            .GetByIdAsync(request.OrganizationId, ct)
            ?? throw new NotFoundException($"Organization {request.OrganizationId} not found");

        // Get quota template by tier name
        var template = await _unitOfWork.Repository<AIQuotaTemplate>()
            .Query()
            .FirstOrDefaultAsync(t => t.TierName == request.TierName && t.IsActive && t.DeletedAt == null, ct)
            ?? throw new NotFoundException($"Active quota template with tier name '{request.TierName}' not found");

        AIQuota quota;

        if (request.ApplyImmediately)
        {
            // Deactivate current quota if exists
            if (currentQuota != null)
            {
                currentQuota.IsActive = false;
                currentQuota.UpdatedAt = DateTimeOffset.UtcNow;
            }

            // Create new quota with custom values or template defaults
            quota = new AIQuota
            {
                OrganizationId = request.OrganizationId,
                TemplateId = template.Id,
                TierName = template.TierName,
                IsActive = true,
                PeriodStartDate = DateTimeOffset.UtcNow,
                PeriodEndDate = DateTimeOffset.UtcNow.AddDays(await GetQuotaPeriodDaysAsync(request.OrganizationId, ct)),
                MaxTokensPerPeriod = request.MaxTokensPerPeriod ?? template.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = request.MaxRequestsPerPeriod ?? template.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod ?? template.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = request.MaxCostPerPeriod ?? template.MaxCostPerPeriod,
                AllowOverage = request.AllowOverage ?? template.AllowOverage,
                OverageRate = request.OverageRate ?? template.OverageRate,
                EnforceQuota = request.EnforceQuota ?? true,
                AlertThresholdPercentage = template.DefaultAlertThresholdPercentage,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // If downgrading and current usage exceeds new limits, mark as exceeded
            if (currentQuota != null)
            {
                quota.TokensUsed = currentQuota.TokensUsed;
                quota.RequestsUsed = currentQuota.RequestsUsed;
                quota.DecisionsMade = currentQuota.DecisionsMade;
                quota.CostAccumulated = currentQuota.CostAccumulated;
                quota.UsageByAgentJson = currentQuota.UsageByAgentJson;
                quota.UsageByDecisionTypeJson = currentQuota.UsageByDecisionTypeJson;
                quota.CheckQuotaExceeded();
            }

            await _unitOfWork.Repository<AIQuota>().AddAsync(quota, ct);
        }
        else
        {
            // Schedule quota change for future date
            var effectiveDate = request.ScheduledDate 
                ?? throw new ValidationException("ScheduledDate is required when ApplyImmediately is false");

            if (effectiveDate <= DateTimeOffset.UtcNow)
            {
                throw new ValidationException("ScheduledDate must be in the future");
            }

            // Create scheduled quota (inactive until EffectiveDate)
            quota = new AIQuota
            {
                OrganizationId = request.OrganizationId,
                TemplateId = template.Id,
                TierName = template.TierName,
                IsActive = false, // Will be activated by background service at EffectiveDate
                EffectiveDate = effectiveDate,
                PeriodStartDate = effectiveDate, // Period starts when quota becomes active
                PeriodEndDate = effectiveDate.AddDays(await GetQuotaPeriodDaysAsync(request.OrganizationId, ct)),
                MaxTokensPerPeriod = request.MaxTokensPerPeriod ?? template.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = request.MaxRequestsPerPeriod ?? template.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod ?? template.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = request.MaxCostPerPeriod ?? template.MaxCostPerPeriod,
                AllowOverage = request.AllowOverage ?? template.AllowOverage,
                OverageRate = request.OverageRate ?? template.OverageRate,
                EnforceQuota = request.EnforceQuota ?? true,
                AlertThresholdPercentage = template.DefaultAlertThresholdPercentage,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // For scheduled quotas, don't copy usage from current quota
            // Usage will start fresh when quota becomes active

            await _unitOfWork.Repository<AIQuota>().AddAsync(quota, ct);

            _logger.LogInformation(
                "Scheduled AI quota change for organization {OrganizationId} to tier {TierName} effective at {EffectiveDate}",
                request.OrganizationId, request.TierName, effectiveDate);
        }

        // Create audit log
        var auditLog = new AuditLog
        {
            UserId = currentUserId,
            Action = "UpdateAIQuota",
            EntityType = "AIQuota",
            EntityId = quota.Id,
            EntityName = $"Quota for {organization.Name}",
            Changes = JsonSerializer.Serialize(new
            {
                OrganizationId = request.OrganizationId,
                OldTier = currentQuota?.TierName,
                NewTier = request.TierName,
                ApplyImmediately = request.ApplyImmediately,
                EffectiveDate = quota.EffectiveDate,
                OldLimits = currentQuota != null ? new
                {
                    currentQuota.MaxTokensPerPeriod,
                    currentQuota.MaxRequestsPerPeriod,
                    currentQuota.MaxDecisionsPerPeriod,
                    currentQuota.MaxCostPerPeriod
                } : null,
                NewLimits = new
                {
                    quota.MaxTokensPerPeriod,
                    quota.MaxRequestsPerPeriod,
                    quota.MaxDecisionsPerPeriod,
                    quota.MaxCostPerPeriod
                },
                Reason = request.Reason
            }),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Send notification email to organization admin
        try
        {
            var orgOwner = await _unitOfWork.Repository<User>()
                .Query()
                .FirstOrDefaultAsync(u => u.OrganizationId == request.OrganizationId && u.GlobalRole == GlobalRole.Admin, ct);

            if (orgOwner != null && !string.IsNullOrEmpty(orgOwner.Email))
            {
                await _emailService.SendAIQuotaUpdatedEmailAsync(
                    orgOwner.Email,
                    organization.Name,
                    currentQuota?.TierName ?? "None",
                    request.TierName,
                    ct
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send quota update email for organization {OrganizationId}", request.OrganizationId);
        }

        _logger.LogInformation("AI quota updated successfully for organization {OrganizationId}", request.OrganizationId);

        var quotaStatus = quota.GetQuotaStatus();
        return new UpdateAIQuotaResponse(
            quota.Id,
            quota.OrganizationId,
            quota.TierName,
            new QuotaLimitsDto(
                quota.MaxTokensPerPeriod,
                quota.MaxRequestsPerPeriod,
                quota.MaxDecisionsPerPeriod,
                quota.MaxCostPerPeriod,
                quota.AllowOverage,
                quota.OverageRate
            ),
            new QuotaStatus(
                quotaStatus.TokensUsed,
                quotaStatus.TokensLimit,
                quotaStatus.TokensPercentage,
                quotaStatus.RequestsUsed,
                quotaStatus.RequestsLimit,
                quotaStatus.RequestsPercentage,
                quotaStatus.CostAccumulated,
                quotaStatus.CostLimit,
                quotaStatus.CostPercentage,
                quotaStatus.IsExceeded,
                quotaStatus.DaysRemaining
            )
        );
    }

    private async Task<int> GetQuotaPeriodDaysAsync(int organizationId, CancellationToken ct)
    {
        return await _settingsService.GetSettingIntAsync(organizationId, "AIQuota.QuotaPeriodDays", ct)
            ?? AIQuotaConstants.QuotaPeriodDays; // Fallback to constant
    }

    private async Task<decimal> GetDefaultAlertThresholdAsync(int organizationId, CancellationToken ct)
    {
        return await _settingsService.GetSettingDecimalAsync(organizationId, "AIQuota.DefaultAlertThreshold", ct)
            ?? AIQuotaConstants.DefaultAlertThreshold; // Fallback to constant
    }
}

