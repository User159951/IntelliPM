using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for disabling AI features for an organization.
/// Emergency kill switch that blocks all AI operations immediately.
/// </summary>
public class DisableAIForOrgCommandHandler : IRequestHandler<DisableAIForOrgCommand, DisableAIForOrgResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DisableAIForOrgCommandHandler> _logger;

    public DisableAIForOrgCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ICacheService cacheService,
        ILogger<DisableAIForOrgCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<DisableAIForOrgResponse> Handle(DisableAIForOrgCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can disable AI for organizations");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        _logger.LogWarning("DISABLING AI for organization {OrganizationId}. Reason: {Reason}",
            request.OrganizationId, request.Reason);

        // Get organization
        var organization = await _unitOfWork.Repository<Organization>()
            .GetByIdAsync(request.OrganizationId, ct)
            ?? throw new NotFoundException($"Organization {request.OrganizationId} not found");

        // Get active quota
        var activeQuota = await _unitOfWork.Repository<AIQuota>()
            .Query()
            .FirstOrDefaultAsync(q => q.OrganizationId == request.OrganizationId && q.IsActive, ct);

        if (activeQuota != null)
        {
            // Deactivate current quota
            activeQuota.IsActive = false;
            activeQuota.EnforceQuota = true;
            activeQuota.IsQuotaExceeded = true;
            activeQuota.QuotaExceededAt = DateTimeOffset.UtcNow;
            activeQuota.QuotaExceededReason = $"AI disabled by admin: {request.Reason}";
            activeQuota.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Get or create a "Disabled" template for disabled quotas
        // Note: In a production system, you might want to ensure this template exists via migration
        var disabledTemplate = await _unitOfWork.Repository<AIQuotaTemplate>()
            .Query()
            .FirstOrDefaultAsync(t => t.TierName == "Disabled" && t.DeletedAt == null, ct);

        // If disabled template doesn't exist, use Free template as fallback (with 0 limits)
        // This should not happen if templates are properly seeded, but handle gracefully
        var templateId = disabledTemplate?.Id ?? 
            (await _unitOfWork.Repository<AIQuotaTemplate>()
                .Query()
                .FirstOrDefaultAsync(t => t.TierName == "Free" && t.IsActive && t.DeletedAt == null, ct))?.Id 
            ?? throw new NotFoundException("No quota templates found. Please ensure templates are seeded.");

        // Create a disabled quota entry as marker
        var disabledQuota = new AIQuota
        {
            OrganizationId = request.OrganizationId,
            TemplateId = templateId,
            TierName = "Disabled",
            IsActive = true,
            EnforceQuota = true,
            IsQuotaExceeded = true,
            QuotaExceededReason = $"AI disabled by admin: {request.Reason}",
            PeriodStartDate = DateTimeOffset.UtcNow,
            PeriodEndDate = DateTimeOffset.MaxValue, // Effectively permanent
            MaxTokensPerPeriod = 0,
            MaxRequestsPerPeriod = 0,
            MaxDecisionsPerPeriod = 0,
            MaxCostPerPeriod = 0m,
            AllowOverage = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Repository<AIQuota>().AddAsync(disabledQuota, ct);

        // Create audit log
        var auditLog = new AuditLog
        {
            UserId = currentUserId,
            Action = "DisableAIForOrganization",
            EntityType = "Organization",
            EntityId = request.OrganizationId,
            EntityName = organization.Name,
            Changes = JsonSerializer.Serialize(new
            {
                OrganizationId = request.OrganizationId,
                OrganizationName = organization.Name,
                Reason = request.Reason,
                Mode = request.Mode.ToString(),
                DisabledAt = DateTimeOffset.UtcNow
            }),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog, ct);

        // Log AI disable event for all future checks
        var disableEvent = new AIDecisionLog
        {
            OrganizationId = request.OrganizationId,
            DecisionId = Guid.NewGuid(),
            DecisionType = "SystemControl",
            AgentType = "System",
            EntityType = "Organization",
            EntityId = request.OrganizationId,
            EntityName = organization.Name,
            Question = "Should AI be enabled for this organization?",
            Decision = "AI disabled by administrator",
            Reasoning = request.Reason,
            ConfidenceScore = 1.0m,
            ModelName = "System",
            ModelVersion = "1.0",
            InputData = JsonSerializer.Serialize(new { OrganizationId = request.OrganizationId }),
            OutputData = JsonSerializer.Serialize(new { Disabled = true, Reason = request.Reason }),
            RequestedByUserId = currentUserId,
            Status = AIDecisionStatus.Applied,
            WasApplied = true,
            AppliedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            IsSuccess = true
        };

        await _unitOfWork.Repository<AIDecisionLog>().AddAsync(disableEvent, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        // Clear cache for this organization's AI status
        await _cacheService.RemoveAsync($"ai_enabled_org_{request.OrganizationId}", ct);

        // Send notification email to organization admins
        if (request.NotifyOrganization)
        {
            try
            {
                var orgAdmins = await _unitOfWork.Repository<User>()
                    .Query()
                    .Where(u => u.OrganizationId == request.OrganizationId && u.GlobalRole == Domain.Enums.GlobalRole.Admin)
                    .ToListAsync(ct);

                foreach (var admin in orgAdmins)
                {
                    if (!string.IsNullOrEmpty(admin.Email))
                    {
                        await _emailService.SendAIDisabledNotificationAsync(
                            admin.Email,
                            organization.Name,
                            request.Reason,
                            request.Mode == DisableMode.Permanent,
                            ct
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send AI disabled notification for organization {OrganizationId}", request.OrganizationId);
            }
        }

        _logger.LogWarning("AI successfully disabled for organization {OrganizationId}", request.OrganizationId);

        return new DisableAIForOrgResponse(
            request.OrganizationId,
            organization.Name,
            true,
            request.Mode,
            DateTimeOffset.UtcNow,
            request.Reason
        );
    }
}

