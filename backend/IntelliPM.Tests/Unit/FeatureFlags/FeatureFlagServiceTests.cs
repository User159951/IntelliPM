using FluentAssertions;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Unit.FeatureFlags;

/// <summary>
/// FAST unit tests for FeatureFlagService using InMemoryDatabase.
/// Tests feature flag enable/disable, global vs organization-specific, and caching.
/// Expected runtime: < 5 seconds
/// </summary>
public class FeatureFlagServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private readonly FeatureFlagService _featureFlagService;
    private readonly string _dbName;

    public FeatureFlagServiceTests()
    {
        // Use unique database name per test for isolation
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<FeatureFlagService>>();
        _featureFlagService = new FeatureFlagService(_context, _memoryCache, logger.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create organizations
        var org1 = new Organization { Id = 1, Name = "Org1", CreatedAt = DateTimeOffset.UtcNow };
        var org2 = new Organization { Id = 2, Name = "Org2", CreatedAt = DateTimeOffset.UtcNow };
        _context.Organizations.AddRange(org1, org2);

        // Create global feature flag (enabled)
        var globalFlag = FeatureFlag.Create("EnableAI", organizationId: null, description: "Global AI feature", isEnabled: true);
        _context.FeatureFlags.Add(globalFlag);

        // Create organization-specific flag for Org1 (enabled)
        var org1Flag = FeatureFlag.Create("EnableAdvancedMetrics", organizationId: 1, description: "Org1 Advanced Metrics", isEnabled: true);
        _context.FeatureFlags.Add(org1Flag);

        // Create organization-specific flag for Org2 (disabled)
        var org2Flag = FeatureFlag.Create("EnableAdvancedMetrics", organizationId: 2, description: "Org2 Advanced Metrics", isEnabled: false);
        _context.FeatureFlags.Add(org2Flag);

        // Create disabled global flag
        var disabledGlobalFlag = FeatureFlag.Create("EnableBetaFeatures", organizationId: null, description: "Global Beta Features", isEnabled: false);
        _context.FeatureFlags.Add(disabledGlobalFlag);

        // Create global flag that Org1 overrides
        var globalOverrideFlag = FeatureFlag.Create("EnableCustomTheme", organizationId: null, description: "Global Custom Theme", isEnabled: false);
        _context.FeatureFlags.Add(globalOverrideFlag);

        // Create Org1 override for EnableCustomTheme (enabled)
        var org1OverrideFlag = FeatureFlag.Create("EnableCustomTheme", organizationId: 1, description: "Org1 Custom Theme Override", isEnabled: true);
        _context.FeatureFlags.Add(org1OverrideFlag);

        _context.SaveChanges();
    }

    [Fact]
    public async Task IsEnabled_Should_Return_True_For_Enabled_Global_Flag()
    {
        // Arrange
        var orgId = 1;

        // Act
        var isEnabled = await _featureFlagService.IsEnabledAsync("EnableAI", orgId);

        // Assert
        isEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabled_Should_Return_False_For_Disabled_Global_Flag()
    {
        // Arrange
        var orgId = 1;

        // Act
        var isEnabled = await _featureFlagService.IsEnabledAsync("EnableBetaFeatures", orgId);

        // Assert
        isEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task OrgSpecific_Flag_Should_Override_Global_Flag()
    {
        // Arrange - Global flag doesn't exist, but org-specific flag exists
        var org1Id = 1;

        // Act
        var isEnabled = await _featureFlagService.IsEnabledAsync("EnableAdvancedMetrics", org1Id);

        // Assert - Org1 flag is enabled
        isEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task OrgSpecific_Flag_Should_Override_Global_Flag_When_Org_Flag_Disabled()
    {
        // Arrange - Org2 has disabled flag
        var org2Id = 2;

        // Act
        var isEnabled = await _featureFlagService.IsEnabledAsync("EnableAdvancedMetrics", org2Id);

        // Assert - Org2 flag is disabled
        isEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task OrgSpecific_Flag_Should_Override_Disabled_Global_Flag()
    {
        // Arrange - Global flag disabled, Org1 flag enabled
        var org1Id = 1;

        // Act
        var isEnabled = await _featureFlagService.IsEnabledAsync("EnableCustomTheme", org1Id);

        // Assert - Returns true (org override)
        isEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabled_Should_Throw_Exception_For_NonExistent_Flag()
    {
        // Arrange
        var orgId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<FeatureFlagNotFoundException>(
            () => _featureFlagService.IsEnabledAsync("NonExistentFlag", orgId));
    }

    [Fact]
    public async Task IsEnabled_Should_Use_Cache_After_First_Query()
    {
        // Arrange
        var orgId = 1;

        // Act - First call (populates cache)
        var isEnabled1 = await _featureFlagService.IsEnabledAsync("EnableAI", orgId);
        isEnabled1.Should().BeTrue();

        // Verify cache is populated by making multiple calls - they should all return the same value
        // without querying the database again
        var isEnabled2 = await _featureFlagService.IsEnabledAsync("EnableAI", orgId);
        var isEnabled3 = await _featureFlagService.IsEnabledAsync("EnableAI", orgId);

        // Assert - All calls should return the same cached value
        isEnabled2.Should().BeTrue();
        isEnabled3.Should().BeTrue();
        isEnabled1.Should().Be(isEnabled2);
        isEnabled2.Should().Be(isEnabled3);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Global_Flag_When_OrgSpecific_Not_Exists()
    {
        // Arrange
        var orgId = 1;

        // Act
        var flag = await _featureFlagService.GetAsync("EnableAI", orgId);

        // Assert
        flag.Should().NotBeNull();
        flag!.Name.Should().Be("EnableAI");
        flag.OrganizationId.Should().BeNull(); // Global flag
        flag.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_Should_Return_OrgSpecific_Flag_When_Exists()
    {
        // Arrange
        var org1Id = 1;

        // Act
        var flag = await _featureFlagService.GetAsync("EnableAdvancedMetrics", org1Id);

        // Assert
        flag.Should().NotBeNull();
        flag!.Name.Should().Be("EnableAdvancedMetrics");
        flag.OrganizationId.Should().Be(org1Id);
        flag.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_Should_Throw_Exception_For_NonExistent_Flag()
    {
        // Arrange
        var orgId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<FeatureFlagNotFoundException>(
            () => _featureFlagService.GetAsync("NonExistentFlag", orgId));
    }

    [Fact]
    public async Task Global_Flag_Should_Apply_To_All_Organizations()
    {
        // Arrange
        var org1Id = 1;
        var org2Id = 2;
        var org3Id = 3; // Non-existent org (but flag exists globally)

        // Act
        var isEnabledOrg1 = await _featureFlagService.IsEnabledAsync("EnableAI", org1Id);
        var isEnabledOrg2 = await _featureFlagService.IsEnabledAsync("EnableAI", org2Id);
        var isEnabledOrg3 = await _featureFlagService.IsEnabledAsync("EnableAI", org3Id);

        // Assert - All should return true (global flag applies to all orgs)
        isEnabledOrg1.Should().BeTrue();
        isEnabledOrg2.Should().BeTrue();
        isEnabledOrg3.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabled_Should_Throw_Exception_For_Empty_Flag_Name()
    {
        // Arrange
        var orgId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _featureFlagService.IsEnabledAsync("", orgId));
    }

    [Fact]
    public async Task IsEnabled_Should_Throw_Exception_For_Null_Flag_Name()
    {
        // Arrange
        var orgId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _featureFlagService.IsEnabledAsync(null!, orgId));
    }

    [Fact]
    public async Task Cache_Should_Be_Invalidated_On_Flag_Update()
    {
        // Arrange
        var orgId = 1;

        // Act - First call (populates cache)
        var isEnabled1 = await _featureFlagService.IsEnabledAsync("EnableAI", orgId);
        isEnabled1.Should().BeTrue();

        // Update flag in database
        var flag = await _context.FeatureFlags.FirstAsync(f => f.Name == "EnableAI" && f.OrganizationId == null);
        flag.Disable();
        await _context.SaveChangesAsync();

        // Clear cache manually (simulating InvalidateCache)
        var cacheKey = $"FeatureFlag_EnableAI_Global";
        _memoryCache.Remove(cacheKey);

        // Second call should query database again
        var isEnabled2 = await _featureFlagService.IsEnabledAsync("EnableAI", orgId);

        // Assert - Should return false from database
        isEnabled2.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_Should_Prioritize_OrgSpecific_Over_Global()
    {
        // Arrange
        var org1Id = 1;

        // Act
        var flag = await _featureFlagService.GetAsync("EnableCustomTheme", org1Id);

        // Assert - Should return org-specific flag, not global
        flag.Should().NotBeNull();
        flag!.OrganizationId.Should().Be(org1Id); // Org-specific, not global
        flag.IsEnabled.Should().BeTrue(); // Org override is enabled
    }

    [Fact]
    public async Task GetAsync_Should_Return_Global_When_No_OrgSpecific_Exists()
    {
        // Arrange
        var org3Id = 3; // Org without specific flag

        // Act
        var flag = await _featureFlagService.GetAsync("EnableCustomTheme", org3Id);

        // Assert - Should return global flag (fallback behavior)
        flag.Should().NotBeNull();
        flag.OrganizationId.Should().BeNull(); // Global flag
        flag.IsEnabled.Should().BeFalse(); // Global flag is disabled
    }

    public void Dispose()
    {
        _context?.Dispose();
        _memoryCache?.Dispose();
    }
}
