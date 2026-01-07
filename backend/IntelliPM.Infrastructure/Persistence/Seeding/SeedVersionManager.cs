using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Persistence.Seeding;

/// <summary>
/// Manages seed script versioning and ensures idempotent seeding.
/// Tracks which seed scripts have been applied and prevents duplicate execution.
/// </summary>
public class SeedVersionManager
{
    private readonly AppDbContext _context;
    private readonly ILogger _logger;

    public SeedVersionManager(AppDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a seed script has already been applied.
    /// </summary>
    public async System.Threading.Tasks.Task<bool> IsSeedAppliedAsync(string seedName, string version)
    {
        return await _context.SeedHistories
            .AnyAsync(sh => sh.SeedName == seedName && sh.Version == version && sh.Success);
    }

    /// <summary>
    /// Records that a seed script has been applied.
    /// </summary>
    public async System.Threading.Tasks.Task RecordSeedAppliedAsync(
        string seedName,
        string version,
        int recordsAffected,
        string? description = null,
        bool success = true,
        string? errorMessage = null)
    {
        var seedHistory = new SeedHistory
        {
            SeedName = seedName,
            Version = version,
            AppliedAt = DateTimeOffset.UtcNow,
            Success = success,
            ErrorMessage = errorMessage,
            RecordsAffected = recordsAffected,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.SeedHistories.Add(seedHistory);
        await _context.SaveChangesAsync();

        if (success)
        {
            _logger.LogInformation(
                "Recorded seed application: {SeedName} v{Version} ({RecordsAffected} records)",
                seedName, version, recordsAffected);
        }
        else
        {
            _logger.LogError(
                "Recorded failed seed application: {SeedName} v{Version} - {ErrorMessage}",
                seedName, version, errorMessage);
        }
    }

    /// <summary>
    /// Applies a seed script if it hasn't been applied yet.
    /// </summary>
    public async System.Threading.Tasks.Task<bool> ApplySeedAsync(
        string seedName,
        string version,
        Func<System.Threading.Tasks.Task<int>> seedAction,
        string? description = null)
    {
        // Check if already applied
        if (await IsSeedAppliedAsync(seedName, version))
        {
            _logger.LogInformation("Seed {SeedName} v{Version} already applied, skipping", seedName, version);
            return false;
        }

        try
        {
            _logger.LogInformation("Applying seed {SeedName} v{Version}...", seedName, version);
            var recordsAffected = await seedAction();
            await RecordSeedAppliedAsync(seedName, version, recordsAffected, description, true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying seed {SeedName} v{Version}", seedName, version);
            await RecordSeedAppliedAsync(seedName, version, 0, description, false, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets all applied seed scripts.
    /// </summary>
    public async System.Threading.Tasks.Task<List<SeedHistory>> GetAppliedSeedsAsync()
    {
        return await _context.SeedHistories
            .OrderByDescending(sh => sh.AppliedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the latest version of a seed script that has been applied.
    /// </summary>
    public async System.Threading.Tasks.Task<string?> GetLatestAppliedVersionAsync(string seedName)
    {
        var latest = await _context.SeedHistories
            .Where(sh => sh.SeedName == seedName && sh.Success)
            .OrderByDescending(sh => sh.AppliedAt)
            .FirstOrDefaultAsync();

        return latest?.Version;
    }

    /// <summary>
    /// Creates a new RBAC policy version snapshot.
    /// </summary>
    public async System.Threading.Tasks.Task<RBACPolicyVersion> CreatePolicyVersionAsync(
        string versionNumber,
        string description,
        int? appliedByUserId = null,
        string? notes = null)
    {
        // Get current permissions and role-permissions
        var permissions = await _context.Permissions
            .Select(p => new { p.Id, p.Name, p.Category, p.Description })
            .ToListAsync();

        var rolePermissions = await _context.RolePermissions
            .Select(rp => new { rp.Role, rp.PermissionId })
            .ToListAsync();

        // Serialize snapshots
        var permissionsSnapshot = System.Text.Json.JsonSerializer.Serialize(permissions);
        var rolePermissionsSnapshot = System.Text.Json.JsonSerializer.Serialize(rolePermissions);

        // Deactivate previous versions
        await _context.RBACPolicyVersions
            .Where(v => v.IsActive)
            .ExecuteUpdateAsync(v => v.SetProperty(x => x.IsActive, false));

        // Create new version
        var policyVersion = new RBACPolicyVersion
        {
            VersionNumber = versionNumber,
            Description = description,
            AppliedAt = DateTimeOffset.UtcNow,
            AppliedByUserId = appliedByUserId,
            PermissionsSnapshotJson = permissionsSnapshot,
            RolePermissionsSnapshotJson = rolePermissionsSnapshot,
            IsActive = true,
            Notes = notes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.RBACPolicyVersions.Add(policyVersion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created RBAC policy version {VersionNumber}: {Description}", versionNumber, description);

        return policyVersion;
    }

    /// <summary>
    /// Gets all RBAC policy versions.
    /// </summary>
    public async System.Threading.Tasks.Task<List<RBACPolicyVersion>> GetPolicyVersionsAsync()
    {
        return await _context.RBACPolicyVersions
            .OrderByDescending(v => v.AppliedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the current active RBAC policy version.
    /// </summary>
    public async System.Threading.Tasks.Task<RBACPolicyVersion?> GetActivePolicyVersionAsync()
    {
        return await _context.RBACPolicyVersions
            .FirstOrDefaultAsync(v => v.IsActive);
    }
}

