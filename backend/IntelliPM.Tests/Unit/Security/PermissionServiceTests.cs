using FluentAssertions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Unit.Security;

/// <summary>
/// FAST unit tests for PermissionService using InMemoryDatabase.
/// Tests permission checks for all GlobalRole and ProjectRole combinations.
/// Expected runtime: < 5 seconds
/// </summary>
public class PermissionServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly PermissionService _permissionService;
    private readonly string _dbName;

    public PermissionServiceTests()
    {
        // Use unique database name per test for isolation
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _mockCacheService = new Mock<ICacheService>();
        _permissionService = new PermissionService(_context, _mockCacheService.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create organizations
        var org1 = new Organization { Id = 1, Name = "Org1", CreatedAt = DateTimeOffset.UtcNow };
        var org2 = new Organization { Id = 2, Name = "Org2", CreatedAt = DateTimeOffset.UtcNow };
        _context.Organizations.AddRange(org1, org2);

        // Create users with different roles
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@test.com",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var regularUser = new User
        {
            Id = 2,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.AddRange(adminUser, regularUser);

        // Create permissions
        var manageUsersPermission = new Permission
        {
            Id = 1,
            Name = "users.manage",
            Category = "Users",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var manageSettingsPermission = new Permission
        {
            Id = 2,
            Name = "settings.manage",
            Category = "Settings",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var viewAllProjectsPermission = new Permission
        {
            Id = 3,
            Name = "projects.view.all",
            Category = "Projects",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var deleteAnyProjectPermission = new Permission
        {
            Id = 4,
            Name = "projects.delete.any",
            Category = "Projects",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createProjectPermission = new Permission
        {
            Id = 5,
            Name = "projects.create",
            Category = "Projects",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Permissions.AddRange(
            manageUsersPermission,
            manageSettingsPermission,
            viewAllProjectsPermission,
            deleteAnyProjectPermission,
            createProjectPermission);

        // Create role permissions - Admin has all permissions
        _context.RolePermissions.AddRange(
            new RolePermission { Id = 1, Role = GlobalRole.Admin, PermissionId = 1, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Id = 2, Role = GlobalRole.Admin, PermissionId = 2, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Id = 3, Role = GlobalRole.Admin, PermissionId = 3, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Id = 4, Role = GlobalRole.Admin, PermissionId = 4, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Id = 5, Role = GlobalRole.Admin, PermissionId = 5, CreatedAt = DateTimeOffset.UtcNow }
        );

        // User role has limited permissions
        _context.RolePermissions.AddRange(
            new RolePermission { Id = 6, Role = GlobalRole.User, PermissionId = 3, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Id = 7, Role = GlobalRole.User, PermissionId = 5, CreatedAt = DateTimeOffset.UtcNow }
        );

        _context.SaveChanges();
    }

    #region GlobalRole Permission Tests

    [Fact]
    public async Task Admin_Should_Have_All_Global_Permissions()
    {
        // Arrange
        var adminUserId = 1;
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var permissions = await _permissionService.GetUserPermissionsAsync(adminUserId);

        // Assert
        permissions.Should().Contain("users.manage");
        permissions.Should().Contain("settings.manage");
        permissions.Should().Contain("projects.view.all");
        permissions.Should().Contain("projects.delete.any");
        permissions.Should().Contain("projects.create");
        permissions.Should().HaveCount(5);
    }

    [Fact]
    public async Task User_Should_Have_Limited_Global_Permissions()
    {
        // Arrange
        var userId = 2;
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var permissions = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        permissions.Should().Contain("projects.view.all");
        permissions.Should().Contain("projects.create");
        permissions.Should().NotContain("users.manage");
        permissions.Should().NotContain("settings.manage");
        permissions.Should().NotContain("projects.delete.any");
        permissions.Should().HaveCount(2);
    }

    [Fact]
    public async Task HasPermission_Should_Return_True_For_Admin_With_ManageUsers()
    {
        // Arrange
        var adminUserId = 1;
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var hasPermission = await _permissionService.HasPermissionAsync(adminUserId, "users.manage");

        // Assert
        hasPermission.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_Should_Return_False_For_User_With_ManageUsers()
    {
        // Arrange
        var userId = 2;
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var hasPermission = await _permissionService.HasPermissionAsync(userId, "users.manage");

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_Should_Return_False_For_Empty_Permission()
    {
        // Arrange
        var adminUserId = 1;

        // Act
        var hasPermission = await _permissionService.HasPermissionAsync(adminUserId, "");

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_Should_Return_False_For_NonExistent_Permission()
    {
        // Arrange
        var adminUserId = 1;
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var hasPermission = await _permissionService.HasPermissionAsync(adminUserId, "non.existent.permission");

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissions_Should_Use_Cache_When_Available()
    {
        // Arrange
        var adminUserId = 1;
        var cachedPermissions = new List<string> { "users.manage", "settings.manage" };
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        // Act
        var permissions = await _permissionService.GetUserPermissionsAsync(adminUserId);

        // Assert
        permissions.Should().BeEquivalentTo(cachedPermissions);
        _mockCacheService.Verify(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserPermissions_Should_Cache_Result_After_First_Query()
    {
        // Arrange
        var adminUserId = 1;
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act - First call
        var permissions1 = await _permissionService.GetUserPermissionsAsync(adminUserId);

        // Assert - Should cache the result
        _mockCacheService.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserPermissions_Should_Return_Empty_List_For_NonExistent_User()
    {
        // Arrange
        var nonExistentUserId = 999;
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var permissions = await _permissionService.GetUserPermissionsAsync(nonExistentUserId);

        // Assert
        permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task HasPermission_Should_Be_Case_Insensitive()
    {
        // Arrange
        var adminUserId = 1;
        _mockCacheService.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var hasPermission1 = await _permissionService.HasPermissionAsync(adminUserId, "users.manage");
        var hasPermission2 = await _permissionService.HasPermissionAsync(adminUserId, "USERS.MANAGE");
        var hasPermission3 = await _permissionService.HasPermissionAsync(adminUserId, "Users.Manage");

        // Assert
        hasPermission1.Should().BeTrue();
        hasPermission2.Should().BeTrue();
        hasPermission3.Should().BeTrue();
    }

    #endregion

    #region GlobalPermissions Static Class Tests

    [Theory]
    [InlineData(GlobalRole.Admin, true)]
    [InlineData(GlobalRole.User, false)]
    public void GlobalPermissions_CanManageUsers_Should_Return_Correct_Value(GlobalRole role, bool expected)
    {
        // Act
        var result = GlobalPermissions.CanManageUsers(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(GlobalRole.Admin, true)]
    [InlineData(GlobalRole.User, false)]
    public void GlobalPermissions_CanManageGlobalSettings_Should_Return_Correct_Value(GlobalRole role, bool expected)
    {
        // Act
        var result = GlobalPermissions.CanManageGlobalSettings(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(GlobalRole.Admin, true)]
    [InlineData(GlobalRole.User, false)]
    public void GlobalPermissions_CanViewAllProjects_Should_Return_Correct_Value(GlobalRole role, bool expected)
    {
        // Act
        var result = GlobalPermissions.CanViewAllProjects(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(GlobalRole.Admin, true)]
    [InlineData(GlobalRole.User, false)]
    public void GlobalPermissions_CanDeleteAnyProject_Should_Return_Correct_Value(GlobalRole role, bool expected)
    {
        // Act
        var result = GlobalPermissions.CanDeleteAnyProject(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void GlobalPermissions_CanAccessSystem_Should_Return_Correct_Value(bool isActive, bool expected)
    {
        // Act
        var result = GlobalPermissions.CanAccessSystem(GlobalRole.User, isActive);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ProjectPermissions Static Class Tests

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, true)]
    [InlineData(ProjectRole.Developer, false)]
    [InlineData(ProjectRole.Tester, false)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanEditProject_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanEditProject(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, false)]
    [InlineData(ProjectRole.Developer, false)]
    [InlineData(ProjectRole.Tester, false)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanDeleteProject_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanDeleteProject(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, true)]
    [InlineData(ProjectRole.Developer, false)]
    [InlineData(ProjectRole.Tester, false)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanInviteMembers_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanInviteMembers(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, true)]
    [InlineData(ProjectRole.Developer, false)]
    [InlineData(ProjectRole.Tester, false)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanRemoveMembers_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanRemoveMembers(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, false)]
    [InlineData(ProjectRole.Developer, false)]
    [InlineData(ProjectRole.Tester, false)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanChangeRoles_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanChangeRoles(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, true)]
    [InlineData(ProjectRole.Developer, true)]
    [InlineData(ProjectRole.Tester, true)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanCreateTasks_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanCreateTasks(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, true)]
    [InlineData(ProjectRole.Developer, true)]
    [InlineData(ProjectRole.Tester, true)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanEditTasks_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanEditTasks(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, true)]
    [InlineData(ProjectRole.Developer, false)]
    [InlineData(ProjectRole.Tester, false)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanDeleteTasks_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanDeleteTasks(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, true)]
    [InlineData(ProjectRole.ScrumMaster, true)]
    [InlineData(ProjectRole.Developer, false)]
    [InlineData(ProjectRole.Tester, false)]
    [InlineData(ProjectRole.Viewer, false)]
    public void ProjectPermissions_CanManageSprints_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanManageSprints(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProjectRole.ProductOwner, false)]
    [InlineData(ProjectRole.ScrumMaster, false)]
    [InlineData(ProjectRole.Developer, false)]
    [InlineData(ProjectRole.Tester, false)]
    [InlineData(ProjectRole.Viewer, true)]
    public void ProjectPermissions_CanViewOnly_Should_Return_Correct_Value(ProjectRole role, bool expected)
    {
        // Act
        var result = ProjectPermissions.CanViewOnly(role);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    public void Dispose()
    {
        _context?.Dispose();
    }
}
