using IntelliPM.Domain.Entities;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Admin controller for data seeding operations.
/// Provides endpoints to trigger seed refreshes and view seed history.
/// SuperAdmin only - seeding operations are sensitive and should be restricted.
/// </summary>
[ApiController]
[Route("api/admin/data-seeding")]
[Authorize(Roles = "SuperAdmin")]
public class DataSeedingController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly SeedVersionManager _versionManager;
    private readonly ILogger<DataSeedingController> _logger;

    public DataSeedingController(
        AppDbContext context,
        SeedVersionManager versionManager,
        ILogger<DataSeedingController> logger)
    {
        _context = context;
        _versionManager = versionManager;
        _logger = logger;
    }

    /// <summary>
    /// Refreshes all seed data (permissions, role-permissions, workflow rules, AI policies).
    /// This will apply any new seeds that haven't been applied yet.
    /// SuperAdmin only.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshSeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshSeeds()
    {
        try
        {
            _logger.LogWarning("Seed refresh triggered by user {UserId}", GetCurrentUserId());

            var results = new List<SeedResult>();

            // 1. Permissions
            var permissionsApplied = await _versionManager.ApplySeedAsync(
                PermissionsSeed.SeedName,
                PermissionsSeed.Version,
                async () => await PermissionsSeed.SeedAsync(_context, _logger),
                "Seeds all system permissions"
            );
            results.Add(new SeedResult
            {
                SeedName = PermissionsSeed.SeedName,
                Version = PermissionsSeed.Version,
                Applied = permissionsApplied
            });

            // 2. Role Permissions
            var rolePermissionsApplied = await _versionManager.ApplySeedAsync(
                RolePermissionsSeed.SeedName,
                RolePermissionsSeed.Version,
                async () => await RolePermissionsSeed.SeedAsync(_context, _logger),
                "Seeds role-permission mappings"
            );
            results.Add(new SeedResult
            {
                SeedName = RolePermissionsSeed.SeedName,
                Version = RolePermissionsSeed.Version,
                Applied = rolePermissionsApplied
            });

            // 3. Workflow Rules
            var workflowRulesApplied = await _versionManager.ApplySeedAsync(
                WorkflowRulesSeed.SeedName,
                WorkflowRulesSeed.Version,
                async () => await WorkflowRulesSeed.SeedAsync(_context, _logger),
                "Seeds workflow transition rules"
            );
            results.Add(new SeedResult
            {
                SeedName = WorkflowRulesSeed.SeedName,
                Version = WorkflowRulesSeed.Version,
                Applied = workflowRulesApplied
            });

            // 4. AI Decision Policies
            var aiPoliciesApplied = await _versionManager.ApplySeedAsync(
                AIDecisionPolicySeed.SeedName,
                AIDecisionPolicySeed.Version,
                async () => await AIDecisionPolicySeed.SeedAsync(_context, _logger),
                "Seeds AI decision approval policies"
            );
            results.Add(new SeedResult
            {
                SeedName = AIDecisionPolicySeed.SeedName,
                Version = AIDecisionPolicySeed.Version,
                Applied = aiPoliciesApplied
            });

            // 5. Create new policy version snapshot
            var newVersion = await _versionManager.CreatePolicyVersionAsync(
                DateTimeOffset.UtcNow.ToString("yyyy.MM.dd.HHmm"),
                $"Policy version created via seed refresh by user {GetCurrentUserId()}",
                GetCurrentUserId(),
                "Created via admin seed refresh endpoint"
            );

            return Ok(new RefreshSeedResponse
            {
                Success = true,
                Message = "Seed refresh completed successfully",
                Results = results,
                PolicyVersionCreated = newVersion.VersionNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing seeds");
            return StatusCode(500, new RefreshSeedResponse
            {
                Success = false,
                Message = $"Error refreshing seeds: {ex.Message}",
                Results = new List<SeedResult>()
            });
        }
    }

    /// <summary>
    /// Gets the seed history (all applied seeds).
    /// SuperAdmin only.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<SeedHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSeedHistory()
    {
        var history = await _versionManager.GetAppliedSeedsAsync();
        var dtos = history.Select(h => new SeedHistoryDto
        {
            Id = h.Id,
            SeedName = h.SeedName,
            Version = h.Version,
            AppliedAt = h.AppliedAt,
            Success = h.Success,
            ErrorMessage = h.ErrorMessage,
            RecordsAffected = h.RecordsAffected,
            Description = h.Description
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Gets all RBAC policy versions.
    /// SuperAdmin only.
    /// </summary>
    [HttpGet("policy-versions")]
    [ProducesResponseType(typeof(List<RBACPolicyVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPolicyVersions()
    {
        var versions = await _versionManager.GetPolicyVersionsAsync();
        var dtos = versions.Select(v => new RBACPolicyVersionDto
        {
            Id = v.Id,
            VersionNumber = v.VersionNumber,
            Description = v.Description,
            AppliedAt = v.AppliedAt,
            AppliedByUserId = v.AppliedByUserId,
            IsActive = v.IsActive,
            Notes = v.Notes,
            CreatedAt = v.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Gets the current active RBAC policy version.
    /// SuperAdmin only.
    /// </summary>
    [HttpGet("policy-versions/active")]
    [ProducesResponseType(typeof(RBACPolicyVersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivePolicyVersion()
    {
        var version = await _versionManager.GetActivePolicyVersionAsync();
        if (version == null)
        {
            return NotFound(new { message = "No active policy version found" });
        }

        var dto = new RBACPolicyVersionDto
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            Description = version.Description,
            AppliedAt = version.AppliedAt,
            AppliedByUserId = version.AppliedByUserId,
            IsActive = version.IsActive,
            Notes = version.Notes,
            CreatedAt = version.CreatedAt
        };

        return Ok(dto);
    }
}

// DTOs
public class RefreshSeedResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<SeedResult> Results { get; set; } = new();
    public string? PolicyVersionCreated { get; set; }
}

public class SeedResult
{
    public string SeedName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool Applied { get; set; }
}

public class SeedHistoryDto
{
    public int Id { get; set; }
    public string SeedName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int RecordsAffected { get; set; }
    public string? Description { get; set; }
}

public class RBACPolicyVersionDto
{
    public int Id { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; set; }
    public int? AppliedByUserId { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

