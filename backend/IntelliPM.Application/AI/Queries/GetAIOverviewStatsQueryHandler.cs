using MediatR;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
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
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

        // Get organization statistics
        var orgRepo = _unitOfWork.Repository<Organization>();
        var totalOrganizations = await orgRepo.Query()
            .AsNoTracking()
            .CountAsync(ct);

        // Organizations with AI enabled (have active quota)
        var quotaRepo = _unitOfWork.Repository<AIQuota>();
        var organizationsWithAIEnabled = await quotaRepo.Query()
            .AsNoTracking()
            .Where(q => q.IsActive)
            .Select(q => q.OrganizationId)
            .Distinct()
            .CountAsync(ct);

        var organizationsWithAIDisabled = totalOrganizations - organizationsWithAIEnabled;

        // Decision statistics from last 30 days
        var decisionRepo = _unitOfWork.Repository<AIDecisionLog>();
        var decisionsLast30Days = await decisionRepo.Query()
            .AsNoTracking()
            .Where(d => d.CreatedAt >= thirtyDaysAgo)
            .ToListAsync(ct);

        var totalDecisionsLast30Days = decisionsLast30Days.Count;

        // Status breakdown
        var pendingApprovals = decisionsLast30Days.Count(d => d.Status == AIDecisionConstants.Statuses.PendingApproval);
        var approvedDecisions = decisionsLast30Days.Count(d => d.Status == AIDecisionConstants.Statuses.Applied);
        var rejectedDecisions = decisionsLast30Days.Count(d => d.Status == AIDecisionConstants.Statuses.Rejected);

        // Average confidence score
        var averageConfidenceScore = decisionsLast30Days.Any()
            ? decisionsLast30Days.Average(d => (double)d.ConfidenceScore)
            : 0.0;

        // Top 5 agents by decision count
        var topAgents = decisionsLast30Days
            .GroupBy(d => d.AgentType)
            .Select(g => new TopAgentUsageDto
            {
                AgentType = g.Key,
                DecisionCount = g.Count(),
                TotalTokensUsed = g.Sum(d => (long)d.TokensUsed)
            })
            .OrderByDescending(a => a.DecisionCount)
            .Take(5)
            .ToList();

        // Quota breakdown by tier
        var quotas = await quotaRepo.Query()
            .AsNoTracking()
            .Include(q => q.Organization)
            .Where(q => q.IsActive)
            .ToListAsync(ct);

        var quotaByTier = quotas
            .GroupBy(q => q.TierName)
            .Select(g =>
            {
                var tierQuotas = g.ToList();
                var statuses = tierQuotas.Select(q => q.GetQuotaStatus()).ToList();
                var avgUsagePercentage = statuses.Any()
                    ? statuses.Average(s => (double)s.TokensPercentage)
                    : 0.0;
                var exceededCount = tierQuotas.Count(q => q.IsQuotaExceeded);

                return new QuotaUsageByTierDto
                {
                    TierName = g.Key,
                    OrganizationCount = g.Count(),
                    AverageUsagePercentage = avgUsagePercentage,
                    ExceededCount = exceededCount
                };
            })
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
            TopAgents = topAgents,
            QuotaByTier = quotaByTier
        };
    }
}

