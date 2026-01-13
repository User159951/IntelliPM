using MediatR;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting AI overview statistics (Admin only).
/// Aggregates statistics from AIDecisionLog and AIQuota entities.
/// </summary>
public class GetAIOverviewStatsQueryHandler : IRequestHandler<GetAIOverviewStatsQuery, AIOverviewStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAIOverviewStatsQueryHandler> _logger;

    public GetAIOverviewStatsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAIOverviewStatsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<AIOverviewStatsDto> Handle(GetAIOverviewStatsQuery request, CancellationToken ct)
    {
        try
        {
            var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

            // Get organization statistics with error handling
            int totalOrganizations = 0;
            try
            {
                var orgRepo = _unitOfWork.Repository<Organization>();
                totalOrganizations = await orgRepo.Query()
                    .AsNoTracking()
                    .CountAsync(ct);
            }
            catch (Exception orgEx)
            {
                _logger.LogWarning(orgEx, "Error querying organizations. Using default value 0.");
                totalOrganizations = 0;
            }

            // Organizations with AI enabled (have active quota) with error handling
            int organizationsWithAIEnabled = 0;
            var quotaRepo = _unitOfWork.Repository<AIQuota>();
            try
            {
                organizationsWithAIEnabled = await quotaRepo.Query()
                    .AsNoTracking()
                    .Where(q => q.IsActive)
                    .Select(q => q.OrganizationId)
                    .Distinct()
                    .CountAsync(ct);
            }
            catch (Exception quotaEx)
            {
                _logger.LogWarning(quotaEx, "Error querying organizations with AI enabled. Using default value 0.");
                organizationsWithAIEnabled = 0;
            }

            var organizationsWithAIDisabled = Math.Max(0, totalOrganizations - organizationsWithAIEnabled);

            // Decision statistics from last 30 days with error handling
            List<AIDecisionLog> decisionsLast30Days = new List<AIDecisionLog>();
            try
            {
                var decisionRepo = _unitOfWork.Repository<AIDecisionLog>();
                decisionsLast30Days = await decisionRepo.Query()
                    .AsNoTracking()
                    .Where(d => d.CreatedAt >= thirtyDaysAgo)
                    .ToListAsync(ct);
            }
            catch (Exception decisionEx)
            {
                _logger.LogWarning(decisionEx, "Error querying AI decision logs. Using empty list.");
                decisionsLast30Days = new List<AIDecisionLog>();
            }

            var totalDecisionsLast30Days = decisionsLast30Days.Count;

            // Status breakdown
            var pendingApprovals = decisionsLast30Days.Count(d => d.Status == AIDecisionStatus.Pending);
            var approvedDecisions = decisionsLast30Days.Count(d => d.Status == AIDecisionStatus.Applied);
            var rejectedDecisions = decisionsLast30Days.Count(d => d.Status == AIDecisionStatus.Rejected);

            // Average confidence score
            var averageConfidenceScore = decisionsLast30Days.Any()
                ? decisionsLast30Days.Average(d => (double)d.ConfidenceScore)
                : 0.0;

            // Total cost for last 30 days
            var totalCostLast30Days = decisionsLast30Days.Sum(d => d.CostAccumulated);

            // Top 5 agents by decision count
            var topAgents = decisionsLast30Days
                .Where(d => !string.IsNullOrEmpty(d.AgentType))
                .GroupBy(d => d.AgentType!)
                .Select(g => new TopAgentUsageDto
                {
                    AgentType = g.Key,
                    DecisionCount = g.Count(),
                    TotalTokensUsed = g.Sum(d => (long)d.TokensUsed)
                })
                .OrderByDescending(a => a.DecisionCount)
                .Take(5)
                .ToList();

            // Quota breakdown by tier with error handling
            List<AIQuota> quotas = new List<AIQuota>();
            try
            {
                quotas = await quotaRepo.Query()
                    .AsNoTracking()
                    .Include(q => q.Organization)
                    .Where(q => q.IsActive)
                    .ToListAsync(ct);
            }
            catch (Exception quotaListEx)
            {
                _logger.LogWarning(quotaListEx, "Error querying quotas. Using empty list.");
                quotas = new List<AIQuota>();
            }

            // Quota breakdown by tier
            var quotaByTier = new List<QuotaUsageByTierDto>();
            var quotasByTier = quotas
                .Where(q => !string.IsNullOrEmpty(q.TierName)) // Filter out quotas with null TierName
                .GroupBy(q => q.TierName!)
                .ToList();

            foreach (var tierGroup in quotasByTier)
            {
                try
                {
                    var tierQuotas = tierGroup.ToList();
                    var statuses = new List<QuotaStatus>();
                    
                    foreach (var quota in tierQuotas)
                    {
                        try
                        {
                            statuses.Add(quota.GetQuotaStatus());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error getting quota status for quota {QuotaId} in tier {TierName}. Exception: {ExceptionType}, Message: {Message}", 
                                quota.Id, tierGroup.Key, ex.GetType().Name, ex.Message);
                            // Skip this quota's status
                        }
                    }
                    
                    var avgUsagePercentage = statuses.Any()
                        ? statuses.Average(s => (double)s.TokensPercentage)
                        : 0.0;
                    
                    // Safely count exceeded quotas
                    var exceededCount = 0;
                    foreach (var quota in tierQuotas)
                    {
                        try
                        {
                            if (quota.IsQuotaExceeded)
                                exceededCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error checking IsQuotaExceeded for quota {QuotaId}", quota.Id);
                        }
                    }

                    quotaByTier.Add(new QuotaUsageByTierDto
                    {
                        TierName = tierGroup.Key,
                        OrganizationCount = tierGroup.Count(),
                        AverageUsagePercentage = avgUsagePercentage,
                        ExceededCount = exceededCount
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing quota tier {TierName}. Exception: {ExceptionType}, Message: {Message}", 
                        tierGroup.Key, ex.GetType().Name, ex.Message);
                    // Skip this tier and continue with others
                }
            }

            quotaByTier = quotaByTier
                .OrderByDescending(q => q.OrganizationCount)
                .ToList();

            _logger.LogInformation(
                "Retrieved AI overview stats: {TotalOrgs} orgs, {EnabledOrgs} enabled, {Decisions30Days} decisions",
                totalOrganizations, organizationsWithAIEnabled, totalDecisionsLast30Days);

            return new AIOverviewStatsDto
            {
                TotalOrganizations = totalOrganizations,
                OrganizationsWithAIEnabled = organizationsWithAIEnabled,
                OrganizationsWithAIDisabled = organizationsWithAIDisabled,
                TotalDecisionsLast30Days = totalDecisionsLast30Days,
                PendingApprovals = pendingApprovals,
                ApprovedDecisions = approvedDecisions,
                RejectedDecisions = rejectedDecisions,
                AverageConfidenceScore = averageConfidenceScore,
                TotalCostLast30Days = totalCostLast30Days,
                TopAgents = topAgents,
                QuotaByTier = quotaByTier
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI overview stats");
            throw;
        }
    }
}

