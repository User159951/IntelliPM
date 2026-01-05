using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Application.Common.Services;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for GetMemberAIQuotasQuery.
/// Returns a paginated list of organization members with their effective AI quotas (Admin only - own organization).
/// Effective quota = UserAIQuota override OR OrganizationAIQuota limit.
/// </summary>
public class GetMemberAIQuotasQueryHandler : IRequestHandler<GetMemberAIQuotasQuery, PagedResponse<MemberAIQuotaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly ILogger<GetMemberAIQuotasQueryHandler> _logger;

    public GetMemberAIQuotasQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        ILogger<GetMemberAIQuotasQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _logger = logger;
    }

    public async Task<PagedResponse<MemberAIQuotaDto>> Handle(GetMemberAIQuotasQuery request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can view member AI quotas");
        }

        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated or organization not found");
        }

        // Get organization AI quota (base limits)
        var orgQuota = await _unitOfWork.Repository<OrganizationAIQuota>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.OrganizationId == organizationId, ct);

        // If no org quota exists, use defaults from constants
        var orgQuotaBase = orgQuota != null
            ? new OrganizationQuotaBaseDto(
                orgQuota.MonthlyTokenLimit,
                orgQuota.MonthlyRequestLimit,
                orgQuota.IsAIEnabled)
            : new OrganizationQuotaBaseDto(
                Domain.Constants.AIQuotaConstants.DefaultLimits[Domain.Constants.AIQuotaConstants.Tiers.Free].MaxTokensPerPeriod,
                Domain.Constants.AIQuotaConstants.DefaultLimits[Domain.Constants.AIQuotaConstants.Tiers.Free].MaxRequestsPerPeriod,
                true); // Default to enabled if no org quota

        // Query users in the organization (with scoping)
        // Apply organization scoping first, then Include
        var userQuery = _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking();
        
        // Apply organization scoping (Admin sees only their org)
        userQuery = _scopingService.ApplyOrganizationScope(userQuery);
        
        // Include after scoping - need to cast to IQueryable first
        IQueryable<User> userQueryWithInclude = userQuery.Include(u => u.Organization);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            userQueryWithInclude = userQueryWithInclude.Where(u =>
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Username.ToLower().Contains(searchTerm));
        }

        // Get total count
        var totalCount = await userQueryWithInclude.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var users = await userQueryWithInclude
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userIds = users.Select(u => u.Id).ToList();

        // Get user quota overrides for these users
        var userQuotas = await _unitOfWork.Repository<UserAIQuota>()
            .Query()
            .AsNoTracking()
            .Where(q => userIds.Contains(q.UserId) && q.OrganizationId == organizationId)
            .ToDictionaryAsync(q => q.UserId, q => q, ct);

        // Build DTOs with effective quota calculation
        var memberDtos = new List<MemberAIQuotaDto>();

        foreach (var user in users)
        {
            var userQuota = userQuotas.GetValueOrDefault(user.Id);

            // Calculate effective quota: UserOverride ?? OrganizationLimit
            var effectiveTokenLimit = userQuota?.MonthlyTokenLimitOverride ?? orgQuotaBase.MonthlyTokenLimit;
            var effectiveRequestLimit = userQuota?.MonthlyRequestLimitOverride ?? orgQuotaBase.MonthlyRequestLimit;
            
            // If org kill-switch is disabled, effective is disabled (unless user has explicit override)
            var effectiveIsAIEnabled = orgQuotaBase.IsAIEnabled
                ? (userQuota?.IsAIEnabledOverride ?? true) // If org enabled, use user override or default to enabled
                : (userQuota?.IsAIEnabledOverride ?? false); // If org disabled, use user override or default to disabled

            var effectiveQuota = new EffectiveMemberQuotaDto(
                effectiveTokenLimit,
                effectiveRequestLimit,
                effectiveIsAIEnabled,
                userQuota != null && (userQuota.MonthlyTokenLimitOverride.HasValue ||
                                     userQuota.MonthlyRequestLimitOverride.HasValue ||
                                     userQuota.IsAIEnabledOverride.HasValue)
            );

            var overrideDto = userQuota != null
                ? new UserQuotaOverrideDto(
                    userQuota.MonthlyTokenLimitOverride,
                    userQuota.MonthlyRequestLimitOverride,
                    userQuota.IsAIEnabledOverride,
                    userQuota.CreatedAt,
                    userQuota.UpdatedAt)
                : null;

            var memberDto = new MemberAIQuotaDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                $"{user.FirstName} {user.LastName}",
                user.GlobalRole.ToString(),
                user.OrganizationId,
                user.Organization.Name,
                effectiveQuota,
                overrideDto,
                orgQuotaBase
            );

            memberDtos.Add(memberDto);
        }

        return new PagedResponse<MemberAIQuotaDto>(
            memberDtos,
            page,
            pageSize,
            totalCount
        );
    }
}

