using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for GetOrganizationAIQuotasQuery.
/// Returns a paginated list of organization AI quotas (SuperAdmin only).
/// </summary>
public class GetOrganizationAIQuotasQueryHandler : IRequestHandler<GetOrganizationAIQuotasQuery, PagedResponse<OrganizationAIQuotaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetOrganizationAIQuotasQueryHandler> _logger;

    public GetOrganizationAIQuotasQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetOrganizationAIQuotasQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResponse<OrganizationAIQuotaDto>> Handle(GetOrganizationAIQuotasQuery request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can list organization AI quotas");
        }

        // Start with organizations query
        var orgQuery = _unitOfWork.Repository<Organization>()
            .Query()
            .AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            orgQuery = orgQuery.Where(o => 
                o.Name.ToLower().Contains(searchTerm) ||
                o.Code.ToLower().Contains(searchTerm));
        }

        // Get organizations with their quotas (left join)
        var orgQuotaQuery = from org in orgQuery
                           join quota in _unitOfWork.Repository<OrganizationAIQuota>().Query().AsNoTracking()
                               on org.Id equals quota.OrganizationId into quotaGroup
                           from quota in quotaGroup.DefaultIfEmpty()
                           select new { Organization = org, Quota = quota };

        // Apply IsAIEnabled filter if specified
        if (request.IsAIEnabled.HasValue)
        {
            orgQuotaQuery = orgQuotaQuery.Where(x => 
                x.Quota == null ? !request.IsAIEnabled.Value : x.Quota.IsAIEnabled == request.IsAIEnabled.Value);
        }

        // Get total count
        var totalCount = await orgQuotaQuery.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var results = await orgQuotaQuery
            .OrderBy(x => x.Organization.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Get default limits for organizations without quotas
        var defaultLimits = Domain.Constants.AIQuotaConstants.DefaultLimits[Domain.Constants.AIQuotaConstants.Tiers.Free];

        var dtos = results.Select(x =>
        {
            if (x.Quota == null)
            {
                // Return default values for organizations without quotas
                return new OrganizationAIQuotaDto(
                    0,
                    x.Organization.Id,
                    x.Organization.Name,
                    x.Organization.Code,
                    defaultLimits.MaxTokensPerPeriod,
                    defaultLimits.MaxRequestsPerPeriod,
                    null,
                    true,
                    x.Organization.CreatedAt,
                    null
                );
            }

            return new OrganizationAIQuotaDto(
                x.Quota.Id,
                x.Quota.OrganizationId,
                x.Organization.Name,
                x.Organization.Code,
                x.Quota.MonthlyTokenLimit,
                x.Quota.MonthlyRequestLimit,
                x.Quota.ResetDayOfMonth,
                x.Quota.IsAIEnabled,
                x.Quota.CreatedAt,
                x.Quota.UpdatedAt
            );
        }).ToList();

        return new PagedResponse<OrganizationAIQuotaDto>(
            dtos,
            page,
            pageSize,
            totalCount
        );
    }
}

