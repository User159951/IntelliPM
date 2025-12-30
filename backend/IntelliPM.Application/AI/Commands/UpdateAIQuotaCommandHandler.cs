using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for updating AI quota limits for organizations.
/// Handles tier changes, billing integration, audit logging, and email notifications.
/// </summary>
public class UpdateAIQuotaCommandHandler : IRequestHandler<UpdateAIQuotaCommand, UpdateAIQuotaResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBillingService _billingService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateAIQuotaCommandHandler> _logger;

    public UpdateAIQuotaCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IBillingService billingService,
        IEmailService emailService,
        ILogger<UpdateAIQuotaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _billingService = billingService;
        _emailService = emailService;
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

        _logger.LogInformation("Updating AI quota for organization {OrganizationId} to tier {TierName}",
            request.OrganizationId, request.TierName);

        // Get current active quota
        var currentQuota = await _unitOfWork.Repository<AIQuota>()
            .Query()
            .FirstOrDefaultAsync(q => q.OrganizationId == request.OrganizationId && q.IsActive, ct);

        var organization = await _unitOfWork.Repository<Organization>()
            .GetByIdAsync(request.OrganizationId, ct)
            ?? throw new NotFoundException($"Organization {request.OrganizationId} not found");

        string? billingReferenceId = null;
        bool wasBillingTriggered = false;

        // Check if tier is changing
        bool isTierChange = currentQuota == null || currentQuota.TierName != request.TierName;

        if (isTierChange)
        {
            // Trigger billing system webhook
            try
            {
                var billingResult = await _billingService.UpdateSubscriptionAsync(new UpdateSubscriptionRequest
                {
                    OrganizationId = request.OrganizationId,
                    OldTier = currentQuota?.TierName ?? "None",
                    NewTier = request.TierName,
                    ApplyImmediately = request.ApplyImmediately,
                    ScheduledDate = request.ScheduledDate
                }, ct);

                if (billingResult.Success)
                {
                    billingReferenceId = billingResult.ReferenceId;
                    wasBillingTriggered = true;
                    _logger.LogInformation("Billing system updated: {ReferenceId}", billingReferenceId);
                }
                else
                {
                    _logger.LogWarning("Billing system update failed: {ErrorMessage}", billingResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update billing system for organization {OrganizationId}", request.OrganizationId);
                // Continue with quota update even if billing fails
            }
        }

        AIQuota quota;

        if (request.ApplyImmediately)
        {
            // Deactivate current quota if exists
            if (currentQuota != null)
            {
                currentQuota.IsActive = false;
                currentQuota.UpdatedAt = DateTimeOffset.UtcNow;
            }

            // Get tier defaults
            var tierLimits = AIQuotaConstants.DefaultLimits.GetValueOrDefault(
                request.TierName,
                AIQuotaConstants.DefaultLimits[AIQuotaConstants.Tiers.Free]
            );

            // Create new quota with custom values or tier defaults
            quota = new AIQuota
            {
                OrganizationId = request.OrganizationId,
                TierName = request.TierName,
                IsActive = true,
                PeriodStartDate = DateTimeOffset.UtcNow,
                PeriodEndDate = DateTimeOffset.UtcNow.AddDays(AIQuotaConstants.QuotaPeriodDays),
                MaxTokensPerPeriod = request.MaxTokensPerPeriod ?? tierLimits.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = request.MaxRequestsPerPeriod ?? tierLimits.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod ?? tierLimits.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = request.MaxCostPerPeriod ?? tierLimits.MaxCostPerPeriod,
                AllowOverage = request.AllowOverage ?? tierLimits.AllowOverage,
                OverageRate = request.OverageRate ?? tierLimits.OverageRate,
                EnforceQuota = request.EnforceQuota ?? true,
                AlertThresholdPercentage = AIQuotaConstants.DefaultAlertThreshold,
                BillingReferenceId = billingReferenceId,
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
            // Schedule quota change
            // TODO: Implement scheduled quota changes (could use Hangfire or similar)
            throw new NotImplementedException("Scheduled quota changes not yet implemented");
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
                Reason = request.Reason,
                BillingReferenceId = billingReferenceId
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
            ),
            wasBillingTriggered,
            billingReferenceId
        );
    }
}

