using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds default workflow transition rules for tasks, sprints, milestones, and releases.
/// Version: 1.0
/// Defines which roles can perform which status transitions.
/// </summary>
public static class WorkflowRulesSeed
{
    public const string SeedName = "WorkflowRulesSeed";
    public const string Version = "1.0";

    /// <summary>
    /// Seeds workflow transition rules into the database.
    /// Idempotent: checks for existing rules before inserting.
    /// </summary>
    public static async Task<int> SeedAsync(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding workflow rules (v{Version})...", Version);

        var existingRules = await context.WorkflowTransitionRules
            .Select(r => new { r.EntityType, r.FromStatus, r.ToStatus })
            .ToListAsync();

        var rules = GetDefaultWorkflowRules();
        var rulesToAdd = rules
            .Where(r => !existingRules.Any(er => 
                er.EntityType == r.EntityType && 
                er.FromStatus == r.FromStatus && 
                er.ToStatus == r.ToStatus))
            .ToList();

        if (rulesToAdd.Any())
        {
            context.WorkflowTransitionRules.AddRange(rulesToAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} new workflow rules (total: {Total}, existing: {Existing})",
                rulesToAdd.Count, rules.Count, existingRules.Count);
        }
        else
        {
            logger.LogInformation("All {Total} workflow rules already exist", rules.Count);
        }

        return rulesToAdd.Count;
    }

    /// <summary>
    /// Gets default workflow transition rules.
    /// </summary>
    private static List<WorkflowTransitionRule> GetDefaultWorkflowRules()
    {
        var now = DateTimeOffset.UtcNow;
        var rules = new List<WorkflowTransitionRule>();

        // ============================================
        // TASK WORKFLOW RULES
        // ============================================
        // Todo -> InProgress (any team member)
        rules.Add(CreateRule("Task", "Todo", "InProgress", 
            new[] { "Developer", "Tester", "ScrumMaster", "ProductOwner" },
            Array.Empty<string>(),
            "Any team member can start working on a task",
            now));

        // Todo -> Blocked (any team member)
        rules.Add(CreateRule("Task", "Todo", "Blocked",
            new[] { "Developer", "Tester", "ScrumMaster", "ProductOwner" },
            Array.Empty<string>(),
            "Any team member can mark a task as blocked",
            now));

        // InProgress -> InReview (Developer, ScrumMaster)
        rules.Add(CreateRule("Task", "InProgress", "InReview",
            new[] { "Developer", "ScrumMaster" },
            Array.Empty<string>(),
            "Developers can mark tasks as ready for review",
            now));

        // InProgress -> Blocked (any team member)
        rules.Add(CreateRule("Task", "InProgress", "Blocked",
            new[] { "Developer", "Tester", "ScrumMaster", "ProductOwner" },
            Array.Empty<string>(),
            "Any team member can block an in-progress task",
            now));

        // InReview -> Done (Tester, ScrumMaster)
        rules.Add(CreateRule("Task", "InReview", "Done",
            new[] { "Tester", "ScrumMaster" },
            Array.Empty<string>(),
            "Testers and Scrum Masters can mark reviewed tasks as done",
            now));

        // InReview -> InProgress (Tester, ScrumMaster)
        rules.Add(CreateRule("Task", "InReview", "InProgress",
            new[] { "Tester", "ScrumMaster" },
            Array.Empty<string>(),
            "Testers can send tasks back to development",
            now));

        // Blocked -> Todo (any team member)
        rules.Add(CreateRule("Task", "Blocked", "Todo",
            new[] { "Developer", "Tester", "ScrumMaster", "ProductOwner" },
            Array.Empty<string>(),
            "Any team member can unblock a task",
            now));

        // Blocked -> InProgress (any team member)
        rules.Add(CreateRule("Task", "Blocked", "InProgress",
            new[] { "Developer", "Tester", "ScrumMaster", "ProductOwner" },
            Array.Empty<string>(),
            "Any team member can resume work on a blocked task",
            now));

        // Done -> InReview (ScrumMaster, ProductOwner) - for corrections
        rules.Add(CreateRule("Task", "Done", "InReview",
            new[] { "ScrumMaster", "ProductOwner" },
            Array.Empty<string>(),
            "Scrum Masters and Product Owners can reopen completed tasks for corrections",
            now));

        // ============================================
        // SPRINT WORKFLOW RULES
        // ============================================
        // NotStarted -> Active (ScrumMaster only)
        rules.Add(CreateRule("Sprint", "NotStarted", "Active",
            new[] { "ScrumMaster" },
            Array.Empty<string>(),
            "Only Scrum Masters can start sprints",
            now));

        // Active -> Completed (ScrumMaster only)
        rules.Add(CreateRule("Sprint", "Active", "Completed",
            new[] { "ScrumMaster" },
            new[] { "AllTasksCompleted" },
            "Scrum Masters can complete sprints when all tasks are done",
            now));

        // Active -> Cancelled (ScrumMaster, ProductOwner)
        rules.Add(CreateRule("Sprint", "Active", "Cancelled",
            new[] { "ScrumMaster", "ProductOwner" },
            Array.Empty<string>(),
            "Scrum Masters and Product Owners can cancel active sprints",
            now));

        // Completed -> Active (ScrumMaster, ProductOwner) - for corrections
        rules.Add(CreateRule("Sprint", "Completed", "Active",
            new[] { "ScrumMaster", "ProductOwner" },
            Array.Empty<string>(),
            "Scrum Masters and Product Owners can reopen completed sprints",
            now));

        // ============================================
        // MILESTONE WORKFLOW RULES
        // ============================================
        // Pending -> InProgress (ProductOwner, ScrumMaster, Manager)
        rules.Add(CreateRule("Milestone", "Pending", "InProgress",
            new[] { "ProductOwner", "ScrumMaster", "Manager" },
            Array.Empty<string>(),
            "Product Owners, Scrum Masters, and Managers can start milestones",
            now));

        // Pending -> Cancelled (ProductOwner, Manager)
        rules.Add(CreateRule("Milestone", "Pending", "Cancelled",
            new[] { "ProductOwner", "Manager" },
            Array.Empty<string>(),
            "Product Owners and Managers can cancel pending milestones",
            now));

        // InProgress -> Completed (ProductOwner, ScrumMaster, Manager)
        rules.Add(CreateRule("Milestone", "InProgress", "Completed",
            new[] { "ProductOwner", "ScrumMaster", "Manager" },
            Array.Empty<string>(),
            "Product Owners, Scrum Masters, and Managers can complete milestones",
            now));

        // InProgress -> Missed (ProductOwner, Manager)
        rules.Add(CreateRule("Milestone", "InProgress", "Missed",
            new[] { "ProductOwner", "Manager" },
            Array.Empty<string>(),
            "Product Owners and Managers can mark milestones as missed",
            now));

        // InProgress -> Cancelled (ProductOwner, Manager)
        rules.Add(CreateRule("Milestone", "InProgress", "Cancelled",
            new[] { "ProductOwner", "Manager" },
            Array.Empty<string>(),
            "Product Owners and Managers can cancel in-progress milestones",
            now));

        // ============================================
        // RELEASE WORKFLOW RULES
        // ============================================
        // Draft -> ReadyForTesting (ProductOwner, ScrumMaster)
        rules.Add(CreateRule("Release", "Draft", "ReadyForTesting",
            new[] { "ProductOwner", "ScrumMaster" },
            Array.Empty<string>(),
            "Product Owners and Scrum Masters can mark releases as ready for testing",
            now));

        // ReadyForTesting -> InTesting (Tester, ScrumMaster)
        rules.Add(CreateRule("Release", "ReadyForTesting", "InTesting",
            new[] { "Tester", "ScrumMaster" },
            Array.Empty<string>(),
            "Testers can start testing releases",
            now));

        // InTesting -> ReadyForDeployment (Tester, ScrumMaster)
        rules.Add(CreateRule("Release", "InTesting", "ReadyForDeployment",
            new[] { "Tester", "ScrumMaster" },
            new[] { "QualityGatesPassed" },
            "Testers can approve releases for deployment after quality gates pass",
            now));

        // ReadyForDeployment -> Deployed (ProductOwner, ScrumMaster)
        rules.Add(CreateRule("Release", "ReadyForDeployment", "Deployed",
            new[] { "ProductOwner", "ScrumMaster" },
            Array.Empty<string>(),
            "Product Owners and Scrum Masters can deploy releases",
            now));

        // InTesting -> Draft (Tester, ScrumMaster) - for fixes
        rules.Add(CreateRule("Release", "InTesting", "Draft",
            new[] { "Tester", "ScrumMaster" },
            Array.Empty<string>(),
            "Testers can send releases back to draft for fixes",
            now));

        // Deployed -> RolledBack (ProductOwner, Manager)
        rules.Add(CreateRule("Release", "Deployed", "RolledBack",
            new[] { "ProductOwner", "Manager" },
            Array.Empty<string>(),
            "Product Owners and Managers can rollback deployed releases",
            now));

        return rules;
    }

    private static WorkflowTransitionRule CreateRule(
        string entityType,
        string fromStatus,
        string toStatus,
        string[] allowedRoles,
        string[] requiredConditions,
        string? description,
        DateTimeOffset createdAt)
    {
        var rule = new WorkflowTransitionRule
        {
            EntityType = entityType,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            IsActive = true,
            Description = description,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
        
        rule.SetAllowedRoles(allowedRoles.ToList());
        rule.SetRequiredConditions(requiredConditions.ToList());
        
        return rule;
    }
}

