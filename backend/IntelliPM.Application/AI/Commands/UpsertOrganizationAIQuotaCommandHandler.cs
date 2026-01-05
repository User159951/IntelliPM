using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for UpsertOrganizationAIQuotaCommand.
/// Creates or updates organization AI quota (SuperAdmin only).
/// </summary>
public class UpsertOrganizationAIQuotaCommandHandler : IRequestHandler<UpsertOrganizationAIQuotaCommand, OrganizationAIQuotaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpsertOrganizationAIQuotaCommandHandler> _logger;

    public UpsertOrganizationAIQuotaCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpsertOrganizationAIQuotaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrganizationAIQuotaDto> Handle(UpsertOrganizationAIQuotaCommand request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can manage organization AI quotas");
        }

        // Verify organization exists
        var organization = await _unitOfWork.Repository<Organization>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, ct);

        if (organization == null)
        {
            throw new NotFoundException($"Organization {request.OrganizationId} not found");
        }

        var quotaRepo = _unitOfWork.Repository<OrganizationAIQuota>();
        var existingQuota = await quotaRepo
            .Query()
            .FirstOrDefaultAsync(q => q.OrganizationId == request.OrganizationId, ct);

        OrganizationAIQuota quota;

        if (existingQuota != null)
        {
            // Update existing quota
            existingQuota.MonthlyTokenLimit = request.MonthlyTokenLimit;
            existingQuota.MonthlyRequestLimit = request.MonthlyRequestLimit;
            existingQuota.ResetDayOfMonth = request.ResetDayOfMonth;
            if (request.IsAIEnabled.HasValue)
            {
                existingQuota.IsAIEnabled = request.IsAIEnabled.Value;
            }
            existingQuota.UpdatedAt = DateTimeOffset.UtcNow;

            quotaRepo.Update(existingQuota);
            quota = existingQuota;

            _logger.LogInformation(
                "Organization AI quota updated for organization {OrganizationId} by SuperAdmin {UserId}",
                request.OrganizationId, _currentUserService.GetUserId());
        }
        else
        {
            // Create new quota
            quota = new OrganizationAIQuota
            {
                OrganizationId = request.OrganizationId,
                MonthlyTokenLimit = request.MonthlyTokenLimit,
                MonthlyRequestLimit = request.MonthlyRequestLimit,
                ResetDayOfMonth = request.ResetDayOfMonth,
                IsAIEnabled = request.IsAIEnabled ?? true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null
            };

            await quotaRepo.AddAsync(quota, ct);

            _logger.LogInformation(
                "Organization AI quota created for organization {OrganizationId} by SuperAdmin {UserId}",
                request.OrganizationId, _currentUserService.GetUserId());
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return new OrganizationAIQuotaDto(
            quota.Id,
            quota.OrganizationId,
            organization.Name,
            organization.Code,
            quota.MonthlyTokenLimit,
            quota.MonthlyRequestLimit,
            quota.ResetDayOfMonth,
            quota.IsAIEnabled,
            quota.CreatedAt,
            quota.UpdatedAt
        );
    }
}

