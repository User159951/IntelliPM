using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Services;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for GetAdminAiQuotaMembersQuery.
/// Returns paginated list of organization members with their AI quota and usage information.
/// </summary>
public class GetAdminAiQuotaMembersQueryHandler : IRequestHandler<GetAdminAiQuotaMembersQuery, PagedResponse<AdminAiQuotaMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly ILogger<GetAdminAiQuotaMembersQueryHandler> _logger;

    public GetAdminAiQuotaMembersQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        ILogger<GetAdminAiQuotaMembersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _logger = logger;
    }

    public async Task<PagedResponse<AdminAiQuotaMemberDto>> Handle(GetAdminAiQuotaMembersQuery request, CancellationToken ct)
    {
        try
        {
            // Verify admin permissions
            if (!_currentUserService.IsAdmin())
            {
                throw new UnauthorizedException("Only administrators can view AI quota members");
            }

        var isSuperAdmin = _currentUserService.IsSuperAdmin();
        
        // Determine target organization ID
        int? targetOrganizationId;
        if (isSuperAdmin)
        {
            // SuperAdmin: use provided organizationId or null (all orgs)
            targetOrganizationId = request.OrganizationId;
        }
        else
        {
            // Admin: use their own organization (ignore provided organizationId)
            var adminOrgId = _currentUserService.GetOrganizationId();
            if (adminOrgId == 0)
            {
                throw new UnauthorizedException("User not authenticated");
            }
            targetOrganizationId = adminOrgId;
        }

        // Get organization's active quota for period calculation
        AIQuota? orgQuota = null;
        DateTimeOffset periodStart;
        DateTimeOffset periodEnd;

        if (targetOrganizationId.HasValue)
        {
            // Get quota for specific organization
            try
            {
                orgQuota = await _unitOfWork.Repository<AIQuota>()
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.OrganizationId == targetOrganizationId.Value && q.IsActive, ct);
            }
            catch (Exception quotaEx)
            {
                _logger.LogWarning(quotaEx, "Error querying AI quota for organization {OrganizationId}. Using default period.", targetOrganizationId.Value);
                orgQuota = null;
            }
        }
        else
        {
            // SuperAdmin viewing all orgs: use first active quota or default period
            try
            {
                orgQuota = await _unitOfWork.Repository<AIQuota>()
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.IsActive, ct);
            }
            catch (Exception quotaEx)
            {
                _logger.LogWarning(quotaEx, "Error querying AI quota for SuperAdmin. Using default period.");
                orgQuota = null;
            }
        }

        if (orgQuota == null)
        {
            // Use default period if no quota exists
            periodStart = DateTimeOffset.UtcNow.AddDays(-30);
            periodEnd = DateTimeOffset.UtcNow.AddDays(30);
        }
        else
        {
            periodStart = orgQuota.PeriodStartDate;
            periodEnd = orgQuota.PeriodEndDate;
        }

        // Query users with organization filtering
        List<User> users;
        int totalCount;
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        try
        {
            var userQuery = _unitOfWork.Repository<User>()
                .Query()
                .AsNoTracking();
            
            // Apply organization filter
            if (targetOrganizationId.HasValue)
            {
                // Filter by specific organization
                userQuery = userQuery.Where(u => u.OrganizationId == targetOrganizationId.Value);
            }
            // If targetOrganizationId is null (SuperAdmin viewing all), don't filter by org
            
            // Then include navigation properties
            userQuery = userQuery.Include(u => u.Organization);
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                userQuery = userQuery.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    (u.FirstName + " " + u.LastName).ToLower().Contains(searchTerm));
            }

            // Get total count
            totalCount = await userQuery.CountAsync(ct);

            // Apply pagination
            users = await userQuery
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }
        catch (Exception userQueryEx)
        {
            _logger.LogError(userQueryEx, "Database error when querying users for AI quota members. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            // Return empty result on database error
            return new PagedResponse<AdminAiQuotaMemberDto>(
                new List<AdminAiQuotaMemberDto>(),
                page,
                pageSize,
                0
            );
        }

        if (users == null || users.Count == 0)
        {
            _logger.LogInformation("No users found for AI quota members query");
            return new PagedResponse<AdminAiQuotaMemberDto>(
                new List<AdminAiQuotaMemberDto>(),
                page,
                pageSize,
                totalCount
            );
        }

        var userIds = users.Select(u => u.Id).ToList();

        // Get quota overrides for these users with error handling
        Dictionary<int, UserAIQuotaOverride> overrides;
        try
        {
            overrides = await _unitOfWork.Repository<UserAIQuotaOverride>()
                .Query()
                .AsNoTracking()
                .Where(o => userIds.Contains(o.UserId) &&
                           o.PeriodStartDate == periodStart &&
                           o.PeriodEndDate == periodEnd)
                .ToDictionaryAsync(o => o.UserId, ct);
        }
        catch (Exception overrideEx)
        {
            _logger.LogWarning(overrideEx, "Error querying quota overrides. Using empty dictionary.");
            overrides = new Dictionary<int, UserAIQuotaOverride>();
        }

        // Get usage counters for these users with error handling
        Dictionary<int, UserAIUsageCounter> usageCounters;
        try
        {
            usageCounters = await _unitOfWork.Repository<UserAIUsageCounter>()
                .Query()
                .AsNoTracking()
                .Where(c => userIds.Contains(c.UserId) &&
                           c.PeriodStartDate == periodStart &&
                           c.PeriodEndDate == periodEnd)
                .ToDictionaryAsync(c => c.UserId, ct);
        }
        catch (Exception counterEx)
        {
            _logger.LogWarning(counterEx, "Error querying usage counters. Using empty dictionary.");
            usageCounters = new Dictionary<int, UserAIUsageCounter>();
        }

        // Build DTOs
        var memberDtos = new List<AdminAiQuotaMemberDto>();

        foreach (var user in users)
        {
            try
            {
                var overrideEntity = overrides.GetValueOrDefault(user.Id);
                var usageCounter = usageCounters.GetValueOrDefault(user.Id);

            // Compute effective quota
            // For SuperAdmin, we may not have an orgQuota, so use defaults
            var effectiveQuota = overrideEntity != null && orgQuota != null
                ? overrideEntity.GetEffectiveLimits(orgQuota)
                : new Domain.Entities.EffectiveQuotaLimits
                {
                    MaxTokensPerPeriod = orgQuota?.MaxTokensPerPeriod ?? 0,
                    MaxRequestsPerPeriod = orgQuota?.MaxRequestsPerPeriod ?? 0,
                    MaxDecisionsPerPeriod = orgQuota?.MaxDecisionsPerPeriod ?? 0,
                    MaxCostPerPeriod = orgQuota?.MaxCostPerPeriod ?? 0,
                    HasOverride = false
                };

            // Build usage DTO (or zeros if no counter exists)
            var usage = usageCounter != null
                ? new UserUsageDto(
                    usageCounter.TokensUsed,
                    usageCounter.RequestsUsed,
                    usageCounter.DecisionsMade,
                    usageCounter.CostAccumulated,
                    effectiveQuota.MaxTokensPerPeriod > 0
                        ? (decimal)usageCounter.TokensUsed / effectiveQuota.MaxTokensPerPeriod * 100
                        : 0,
                    effectiveQuota.MaxRequestsPerPeriod > 0
                        ? (decimal)usageCounter.RequestsUsed / effectiveQuota.MaxRequestsPerPeriod * 100
                        : 0,
                    effectiveQuota.MaxDecisionsPerPeriod > 0
                        ? (decimal)usageCounter.DecisionsMade / effectiveQuota.MaxDecisionsPerPeriod * 100
                        : 0,
                    effectiveQuota.MaxCostPerPeriod > 0
                        ? usageCounter.CostAccumulated / effectiveQuota.MaxCostPerPeriod * 100
                        : 0
                )
                : new UserUsageDto(0, 0, 0, 0, 0, 0, 0, 0);

            // Build override DTO
            var overrideDto = overrideEntity != null
                ? new QuotaOverrideDto(
                    overrideEntity.MaxTokensPerPeriod,
                    overrideEntity.MaxRequestsPerPeriod,
                    overrideEntity.MaxDecisionsPerPeriod,
                    overrideEntity.MaxCostPerPeriod,
                    overrideEntity.CreatedAt,
                    overrideEntity.Reason
                )
                : null;

            var memberDto = new AdminAiQuotaMemberDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                $"{user.FirstName} {user.LastName}",
                user.GlobalRole.ToString(),
                user.OrganizationId,
                user.Organization?.Name ?? "Unknown Organization",
                new EffectiveQuotaDto(
                    effectiveQuota.MaxTokensPerPeriod,
                    effectiveQuota.MaxRequestsPerPeriod,
                    effectiveQuota.MaxDecisionsPerPeriod,
                    effectiveQuota.MaxCostPerPeriod,
                    effectiveQuota.HasOverride
                ),
                overrideDto,
                usage,
                new PeriodInfoDto(
                    periodStart,
                    periodEnd,
                    Math.Max(0, (periodEnd.Date - DateTimeOffset.UtcNow.Date).Days)
                )
            );

                memberDtos.Add(memberDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user {UserId} in AI quota members query. User email: {Email}, OrganizationId: {OrgId}", 
                    user.Id, user.Email, user.OrganizationId);
                // Skip this user and continue with others
                continue;
            }
        }

        return new PagedResponse<AdminAiQuotaMemberDto>(
            memberDtos,
            page,
            pageSize,
            totalCount
        );
        }
        catch (UnauthorizedException)
        {
            // Re-throw authorization exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAdminAiQuotaMembersQueryHandler. Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", 
                request.Page, request.PageSize, request.SearchTerm);
            throw;
        }
    }
}

