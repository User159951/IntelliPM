using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds default AI decision approval policies.
/// Version: 1.0
/// Defines which roles must approve different types of AI decisions.
/// </summary>
public static class AIDecisionPolicySeed
{
    public const string SeedName = "AIDecisionPolicySeed";
    public const string Version = "1.0";

    /// <summary>
    /// Seeds AI decision approval policies into the database.
    /// Idempotent: checks for existing policies before inserting.
    /// </summary>
    public static async Task<int> SeedAsync(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding AI decision approval policies (v{Version})...", Version);

        var existingPolicies = await context.AIDecisionApprovalPolicies
            .Where(p => p.OrganizationId == null) // Only check global policies
            .Select(p => p.DecisionType)
            .ToListAsync();

        var policies = GetDefaultPolicies();
        var policiesToAdd = policies
            .Where(p => !existingPolicies.Contains(p.DecisionType))
            .ToList();

        if (policiesToAdd.Any())
        {
            context.AIDecisionApprovalPolicies.AddRange(policiesToAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} new AI decision approval policies (total: {Total}, existing: {Existing})",
                policiesToAdd.Count, policies.Count, existingPolicies.Count);
        }
        else
        {
            logger.LogInformation("All {Total} AI decision approval policies already exist", policies.Count);
        }

        return policiesToAdd.Count;
    }

    /// <summary>
    /// Gets default AI decision approval policies.
    /// </summary>
    private static List<AIDecisionApprovalPolicy> GetDefaultPolicies()
    {
        var now = DateTimeOffset.UtcNow;
        var policies = new List<AIDecisionApprovalPolicy>();

        // Critical decisions require ProductOwner or Admin approval
        policies.Add(new AIDecisionApprovalPolicy
        {
            OrganizationId = null, // Global policy
            DecisionType = "CriticalSystemDecision",
            RequiredRole = "ProductOwner",
            IsBlockingIfNotApproved = true,
            IsActive = true,
            Description = "Critical system decisions require ProductOwner or Admin approval before execution",
            CreatedAt = now
        });

        // Cost decisions require Admin or ProductOwner approval
        policies.Add(new AIDecisionApprovalPolicy
        {
            OrganizationId = null,
            DecisionType = "CostDecision",
            RequiredRole = "Admin",
            IsBlockingIfNotApproved = true,
            IsActive = true,
            Description = "Cost-related decisions require Admin or ProductOwner approval",
            CreatedAt = now
        });

        // Quota decisions require Admin approval
        policies.Add(new AIDecisionApprovalPolicy
        {
            OrganizationId = null,
            DecisionType = "QuotaDecision",
            RequiredRole = "Admin",
            IsBlockingIfNotApproved = true,
            IsActive = true,
            Description = "Quota modification decisions require Admin approval",
            CreatedAt = now
        });

        // Risk detection decisions require ProductOwner or ScrumMaster acknowledgment (non-blocking)
        policies.Add(new AIDecisionApprovalPolicy
        {
            OrganizationId = null,
            DecisionType = "RiskDetection",
            RequiredRole = "ProductOwner",
            IsBlockingIfNotApproved = false,
            IsActive = true,
            Description = "Risk detection decisions should be acknowledged by ProductOwner or ScrumMaster (non-blocking)",
            CreatedAt = now
        });

        // Sprint planning decisions require ScrumMaster approval
        policies.Add(new AIDecisionApprovalPolicy
        {
            OrganizationId = null,
            DecisionType = "SprintPlanning",
            RequiredRole = "ScrumMaster",
            IsBlockingIfNotApproved = true,
            IsActive = true,
            Description = "Sprint planning decisions require ScrumMaster approval",
            CreatedAt = now
        });

        // Task prioritization decisions require ProductOwner approval
        policies.Add(new AIDecisionApprovalPolicy
        {
            OrganizationId = null,
            DecisionType = "TaskPrioritization",
            RequiredRole = "ProductOwner",
            IsBlockingIfNotApproved = false,
            IsActive = true,
            Description = "Task prioritization decisions should be reviewed by ProductOwner (non-blocking)",
            CreatedAt = now
        });

        // Resource allocation decisions require Manager or Admin approval
        policies.Add(new AIDecisionApprovalPolicy
        {
            OrganizationId = null,
            DecisionType = "ResourceAllocation",
            RequiredRole = "Manager",
            IsBlockingIfNotApproved = true,
            IsActive = true,
            Description = "Resource allocation decisions require Manager or Admin approval",
            CreatedAt = now
        });

        // Quality gate decisions require Tester approval
        policies.Add(new AIDecisionApprovalPolicy
        {
            OrganizationId = null,
            DecisionType = "QualityGateDecision",
            RequiredRole = "Tester",
            IsBlockingIfNotApproved = true,
            IsActive = true,
            Description = "Quality gate decisions require Tester approval",
            CreatedAt = now
        });

        return policies;
    }
}

