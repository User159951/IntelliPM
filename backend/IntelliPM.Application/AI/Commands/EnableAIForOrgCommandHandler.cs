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
/// Handler for re-enabling AI features for an organization.
/// Removes disabled quota and creates new quota with specified tier.
/// </summary>
public class EnableAIForOrgCommandHandler : IRequestHandler<EnableAIForOrgCommand, EnableAIForOrgResponse>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<EnableAIForOrgCommandHandler> _logger;

    public EnableAIForOrgCommandHandler(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        ILogger<EnableAIForOrgCommandHandler> logger)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<EnableAIForOrgResponse> Handle(EnableAIForOrgCommand request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can enable AI for organizations");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        _logger.LogInformation("Enabling AI for organization {OrganizationId}", request.OrganizationId);

        var organization = await _unitOfWork.Repository<Organization>()
            .GetByIdAsync(request.OrganizationId, ct)
            ?? throw new NotFoundException($"Organization {request.OrganizationId} not found");

        // Remove disabled quota
        var disabledQuota = await _unitOfWork.Repository<AIQuota>()
            .Query()
            .FirstOrDefaultAsync(q => q.OrganizationId == request.OrganizationId && q.TierName == "Disabled" && q.IsActive, ct);

        if (disabledQuota != null)
        {
            disabledQuota.IsActive = false;
            disabledQuota.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Create new quota with specified tier using UpdateAIQuotaCommand
        var updateQuotaCommand = new UpdateAIQuotaCommand
        {
            OrganizationId = request.OrganizationId,
            TierName = request.TierName,
            ApplyImmediately = true,
            Reason = $"AI re-enabled: {request.Reason}"
        };

        await _mediator.Send(updateQuotaCommand, ct);

        // Clear cache
        await _cacheService.RemoveAsync($"ai_enabled_org_{request.OrganizationId}", ct);

        // Create audit log
        var auditLog = new AuditLog
        {
            UserId = currentUserId,
            Action = "EnableAIForOrganization",
            EntityType = "Organization",
            EntityId = request.OrganizationId,
            EntityName = organization.Name,
            Changes = JsonSerializer.Serialize(new
            {
                OrganizationId = request.OrganizationId,
                OrganizationName = organization.Name,
                TierName = request.TierName,
                Reason = request.Reason,
                EnabledAt = DateTimeOffset.UtcNow
            }),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("AI successfully enabled for organization {OrganizationId}", request.OrganizationId);

        return new EnableAIForOrgResponse(
            request.OrganizationId,
            organization.Name,
            true,
            request.TierName
        );
    }
}

