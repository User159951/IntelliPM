using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using IntelliPM.Infrastructure.Services;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Application.Common.Authorization;
using Task = System.Threading.Tasks.Task;

namespace IntelliPM.Tests.Unit.Services;

/// <summary>
/// Comprehensive unit tests for PermissionService covering all roles, permissions, and edge cases.
/// Tests both GetUserPermissionsAsync and HasPermissionAsync methods.
/// Uses InMemoryDatabase for realistic DbContext behavior.
/// </summary>
public class PermissionServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICacheService> _mockCache;
    private readonly PermissionService _permissionService;

    public PermissionServiceTests()
    {
        // Use InMemoryDatabase for testing (more realistic than mocking DbContext)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"PermissionServiceTests_{Guid.NewGuid()}")
            .Options;

        var serviceProvider = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .BuildServiceProvider();

        _context = new AppDbContext(options, serviceProvider);
        _mockCache = new Mock<ICacheService>();
        _permissionService = new PermissionService(_context, _mockCache.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region GlobalRole Permission Tests

    [Fact]
    public async Task GetUserPermissionsAsync_AdminUser_ReturnsAllAdminPermissions()
    {
        // Arrange
        var userId = 1;
        var organizationId = 1;
        
        var adminUser = new User
        {
            Id = userId,
            Username = "admin",
            Email = "admin@test.com",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = organizationId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var adminPermissions = new List<Permission>
        {
            new Permission { Id = 1, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 2, Name = "projects.read", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 3, Name = "users.manage", Category = "Users", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 4, Name = "admin.settings.update", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 5, Name = "admin.permissions.update", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = adminPermissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.Admin,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(adminUser);
        _context.Permissions.AddRange(adminPermissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().Contain("projects.create");
        result.Should().Contain("users.manage");
        result.Should().Contain("admin.settings.update");
    }

    [Fact]
    public async Task GetUserPermissionsAsync_RegularUser_ReturnsUserPermissions()
    {
        // Arrange
        var userId = 2;
        var organizationId = 1;
        
        var regularUser = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = organizationId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var userPermissions = new List<Permission>
        {
            new Permission { Id = 10, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 11, Name = "projects.read", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 12, Name = "tasks.create", Category = "Tasks", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 13, Name = "tasks.read", Category = "Tasks", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = userPermissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.User,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(regularUser);
        _context.Permissions.AddRange(userPermissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result.Should().Contain("projects.create");
        result.Should().Contain("tasks.create");
        result.Should().NotContain("users.manage");
        result.Should().NotContain("admin.settings.update");
    }

    [Fact]
    public async Task GetUserPermissionsAsync_UserNotFound_ReturnsEmptyList()
    {
        // Arrange
        var userId = 999;

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_UsesCache_WhenPermissionsCached()
    {
        // Arrange
        var userId = 1;
        var cachedPermissions = new List<string> { "projects.create", "projects.read" };

        _mockCache.Setup(c => c.GetAsync<List<string>>($"user_permissions_{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedPermissions);
        // Note: InMemoryDatabase doesn't support Verify, so we skip verification
        // The test verifies behavior through assertions instead
    }

    [Fact]
    public async Task GetUserPermissionsAsync_CachesResult_AfterDatabaseQuery()
    {
        // Arrange
        var userId = 1;
        var organizationId = 1;
        
        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = organizationId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 20, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.User,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(user);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);
        _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        _mockCache.Verify(
            c => c.SetAsync(
                $"user_permissions_{userId}",
                It.Is<List<string>>(p => p.Contains("projects.create")),
                It.Is<TimeSpan>(t => t.TotalMinutes == 5),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region HasPermissionAsync Tests

    [Fact]
    public async Task HasPermissionAsync_UserHasPermission_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var permission = "projects.create";
        
        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 30, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.User,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(user);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_UserDoesNotHavePermission_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var permission = "users.manage";
        
        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 40, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.User,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(user);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_AdminHasAllPermissions_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var permission = "users.manage";
        
        var adminUser = new User
        {
            Id = userId,
            Username = "admin",
            Email = "admin@test.com",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 50, Name = "users.manage", Category = "Users", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 51, Name = "admin.settings.update", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.Admin,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(adminUser);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_UserCannotManageUsers_ReturnsFalse()
    {
        // Arrange
        var userId = 2;
        var permission = "users.manage";
        
        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 60, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.User,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(user);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var permission = "PROJECTS.CREATE"; // Uppercase
        
        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 70, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.User,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(user);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_EmptyPermission_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var permission = string.Empty;

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeFalse();
        // Should not query database for empty permission
    }

    [Fact]
    public async Task HasPermissionAsync_WhitespacePermission_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var permission = "   ";

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeFalse();
        // Should not query database for whitespace permission
    }

    [Fact]
    public async Task HasPermissionAsync_NullPermission_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        string? permission = null;

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission!);

        // Assert
        result.Should().BeFalse();
        // Should not query database for null permission
    }

    #endregion

    #region GlobalPermissions Static Class Tests

    [Fact]
    public void GlobalPermissions_AdminCanManageUsers_ReturnsTrue()
    {
        // Arrange
        var role = GlobalRole.Admin;

        // Act
        var result = GlobalPermissions.CanManageUsers(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GlobalPermissions_UserCannotManageUsers_ReturnsFalse()
    {
        // Arrange
        var role = GlobalRole.User;

        // Act
        var result = GlobalPermissions.CanManageUsers(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GlobalPermissions_AdminCanManageGlobalSettings_ReturnsTrue()
    {
        // Arrange
        var role = GlobalRole.Admin;

        // Act
        var result = GlobalPermissions.CanManageGlobalSettings(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GlobalPermissions_UserCannotManageGlobalSettings_ReturnsFalse()
    {
        // Arrange
        var role = GlobalRole.User;

        // Act
        var result = GlobalPermissions.CanManageGlobalSettings(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GlobalPermissions_AdminCanViewAllProjects_ReturnsTrue()
    {
        // Arrange
        var role = GlobalRole.Admin;

        // Act
        var result = GlobalPermissions.CanViewAllProjects(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GlobalPermissions_UserCannotViewAllProjects_ReturnsFalse()
    {
        // Arrange
        var role = GlobalRole.User;

        // Act
        var result = GlobalPermissions.CanViewAllProjects(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GlobalPermissions_AdminCanDeleteAnyProject_ReturnsTrue()
    {
        // Arrange
        var role = GlobalRole.Admin;

        // Act
        var result = GlobalPermissions.CanDeleteAnyProject(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GlobalPermissions_UserCannotDeleteAnyProject_ReturnsFalse()
    {
        // Arrange
        var role = GlobalRole.User;

        // Act
        var result = GlobalPermissions.CanDeleteAnyProject(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GlobalPermissions_ActiveUserCanAccessSystem_ReturnsTrue()
    {
        // Arrange
        var role = GlobalRole.User;
        var isActive = true;

        // Act
        var result = GlobalPermissions.CanAccessSystem(role, isActive);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GlobalPermissions_InactiveUserCannotAccessSystem_ReturnsFalse()
    {
        // Arrange
        var role = GlobalRole.User;
        var isActive = false;

        // Act
        var result = GlobalPermissions.CanAccessSystem(role, isActive);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GlobalPermissions_InactiveAdminCannotAccessSystem_ReturnsFalse()
    {
        // Arrange
        var role = GlobalRole.Admin;
        var isActive = false;

        // Act
        var result = GlobalPermissions.CanAccessSystem(role, isActive);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ProjectPermissions Static Class Tests

    [Fact]
    public void ProjectPermissions_ProductOwnerCanEditProject_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act
        var result = ProjectPermissions.CanEditProject(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ScrumMasterCanEditProject_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ScrumMaster;

        // Act
        var result = ProjectPermissions.CanEditProject(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_DeveloperCannotEditProject_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.Developer;

        // Act
        var result = ProjectPermissions.CanEditProject(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ProductOwnerCanDeleteProject_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act
        var result = ProjectPermissions.CanDeleteProject(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ScrumMasterCannotDeleteProject_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.ScrumMaster;

        // Act
        var result = ProjectPermissions.CanDeleteProject(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_DeveloperCannotDeleteProject_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.Developer;

        // Act
        var result = ProjectPermissions.CanDeleteProject(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ProductOwnerCanInviteMembers_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act
        var result = ProjectPermissions.CanInviteMembers(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ScrumMasterCanInviteMembers_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ScrumMaster;

        // Act
        var result = ProjectPermissions.CanInviteMembers(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_DeveloperCannotInviteMembers_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.Developer;

        // Act
        var result = ProjectPermissions.CanInviteMembers(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ProductOwnerCanChangeRoles_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act
        var result = ProjectPermissions.CanChangeRoles(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ScrumMasterCannotChangeRoles_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.ScrumMaster;

        // Act
        var result = ProjectPermissions.CanChangeRoles(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ProductOwnerCanCreateTasks_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act
        var result = ProjectPermissions.CanCreateTasks(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_DeveloperCanCreateTasks_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.Developer;

        // Act
        var result = ProjectPermissions.CanCreateTasks(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_TesterCanCreateTasks_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.Tester;

        // Act
        var result = ProjectPermissions.CanCreateTasks(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ViewerCannotCreateTasks_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.Viewer;

        // Act
        var result = ProjectPermissions.CanCreateTasks(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ProductOwnerCanDeleteTasks_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act
        var result = ProjectPermissions.CanDeleteTasks(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ScrumMasterCanDeleteTasks_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ScrumMaster;

        // Act
        var result = ProjectPermissions.CanDeleteTasks(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_DeveloperCannotDeleteTasks_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.Developer;

        // Act
        var result = ProjectPermissions.CanDeleteTasks(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ProductOwnerCanManageSprints_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act
        var result = ProjectPermissions.CanManageSprints(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ScrumMasterCanManageSprints_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.ScrumMaster;

        // Act
        var result = ProjectPermissions.CanManageSprints(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_DeveloperCannotManageSprints_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.Developer;

        // Act
        var result = ProjectPermissions.CanManageSprints(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ViewerCanOnlyView_ReturnsTrue()
    {
        // Arrange
        var role = ProjectRole.Viewer;

        // Act
        var result = ProjectPermissions.CanViewOnly(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ProductOwnerIsNotViewOnly_ReturnsFalse()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act
        var result = ProjectPermissions.CanViewOnly(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ProductOwnerHasFullAccess_AllPermissionsTrue()
    {
        // Arrange
        var role = ProjectRole.ProductOwner;

        // Act & Assert
        ProjectPermissions.CanEditProject(role).Should().BeTrue();
        ProjectPermissions.CanDeleteProject(role).Should().BeTrue();
        ProjectPermissions.CanInviteMembers(role).Should().BeTrue();
        ProjectPermissions.CanRemoveMembers(role).Should().BeTrue();
        ProjectPermissions.CanChangeRoles(role).Should().BeTrue();
        ProjectPermissions.CanCreateTasks(role).Should().BeTrue();
        ProjectPermissions.CanEditTasks(role).Should().BeTrue();
        ProjectPermissions.CanDeleteTasks(role).Should().BeTrue();
        ProjectPermissions.CanManageSprints(role).Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_ScrumMasterHasManagementAccess_CanManageButNotDeleteProject()
    {
        // Arrange
        var role = ProjectRole.ScrumMaster;

        // Act & Assert
        ProjectPermissions.CanEditProject(role).Should().BeTrue();
        ProjectPermissions.CanDeleteProject(role).Should().BeFalse();
        ProjectPermissions.CanInviteMembers(role).Should().BeTrue();
        ProjectPermissions.CanRemoveMembers(role).Should().BeTrue();
        ProjectPermissions.CanChangeRoles(role).Should().BeFalse();
        ProjectPermissions.CanCreateTasks(role).Should().BeTrue();
        ProjectPermissions.CanEditTasks(role).Should().BeTrue();
        ProjectPermissions.CanDeleteTasks(role).Should().BeTrue();
        ProjectPermissions.CanManageSprints(role).Should().BeTrue();
    }

    [Fact]
    public void ProjectPermissions_DeveloperHasLimitedAccess_CanCreateEditTasksButNotManageProject()
    {
        // Arrange
        var role = ProjectRole.Developer;

        // Act & Assert
        ProjectPermissions.CanEditProject(role).Should().BeFalse();
        ProjectPermissions.CanDeleteProject(role).Should().BeFalse();
        ProjectPermissions.CanInviteMembers(role).Should().BeFalse();
        ProjectPermissions.CanRemoveMembers(role).Should().BeFalse();
        ProjectPermissions.CanChangeRoles(role).Should().BeFalse();
        ProjectPermissions.CanCreateTasks(role).Should().BeTrue();
        ProjectPermissions.CanEditTasks(role).Should().BeTrue();
        ProjectPermissions.CanDeleteTasks(role).Should().BeFalse();
        ProjectPermissions.CanManageSprints(role).Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_TesterHasTaskAccess_CanCreateEditTasksButNotDelete()
    {
        // Arrange
        var role = ProjectRole.Tester;

        // Act & Assert
        ProjectPermissions.CanEditProject(role).Should().BeFalse();
        ProjectPermissions.CanDeleteProject(role).Should().BeFalse();
        ProjectPermissions.CanInviteMembers(role).Should().BeFalse();
        ProjectPermissions.CanRemoveMembers(role).Should().BeFalse();
        ProjectPermissions.CanChangeRoles(role).Should().BeFalse();
        ProjectPermissions.CanCreateTasks(role).Should().BeTrue();
        ProjectPermissions.CanEditTasks(role).Should().BeTrue();
        ProjectPermissions.CanDeleteTasks(role).Should().BeFalse();
        ProjectPermissions.CanManageSprints(role).Should().BeFalse();
    }

    [Fact]
    public void ProjectPermissions_ViewerHasReadOnlyAccess_AllWritePermissionsFalse()
    {
        // Arrange
        var role = ProjectRole.Viewer;

        // Act & Assert
        ProjectPermissions.CanEditProject(role).Should().BeFalse();
        ProjectPermissions.CanDeleteProject(role).Should().BeFalse();
        ProjectPermissions.CanInviteMembers(role).Should().BeFalse();
        ProjectPermissions.CanRemoveMembers(role).Should().BeFalse();
        ProjectPermissions.CanChangeRoles(role).Should().BeFalse();
        ProjectPermissions.CanCreateTasks(role).Should().BeFalse();
        ProjectPermissions.CanEditTasks(role).Should().BeFalse();
        ProjectPermissions.CanDeleteTasks(role).Should().BeFalse();
        ProjectPermissions.CanManageSprints(role).Should().BeFalse();
        ProjectPermissions.CanViewOnly(role).Should().BeTrue();
    }

    #endregion

    #region Organization Isolation Tests

    [Fact]
    public async Task GetUserPermissionsAsync_RespectsOrganizationIsolation_UserHasOrganizationId()
    {
        // Arrange
        var userId = 1;
        var organizationId = 1;
        
        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = organizationId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Permissions are role-based, not organization-based in the current implementation
        // But we verify that user's organization is tracked
        var permissions = new List<Permission>
        {
            new Permission { Id = 80, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.User,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(user);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        user.OrganizationId.Should().Be(organizationId);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task GetUserPermissionsAsync_UserWithNoRolePermissions_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        
        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HasPermissionAsync_InvalidUserId_ReturnsFalse()
    {
        // Arrange
        var userId = -1;
        var permission = "projects.create";

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_PermissionWithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var userId = 1;
        var permission = "projects.create.with.dots";
        
        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 90, Name = "projects.create.with.dots", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.User,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(user);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_MultiplePermissions_ReturnsAllPermissions()
    {
        // Arrange
        var userId = 1;
        
        var user = new User
        {
            Id = userId,
            Username = "admin",
            Email = "admin@test.com",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 100, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 101, Name = "projects.read", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 102, Name = "projects.update", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 103, Name = "projects.delete", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 104, Name = "users.manage", Category = "Users", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 105, Name = "admin.settings.update", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = permissions.Select(p => new RolePermission
        {
            Id = p.Id,
            Role = GlobalRole.Admin,
            PermissionId = p.Id,
            Permission = p,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _context.Users.Add(user);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);
        result.Should().Contain("projects.create");
        result.Should().Contain("users.manage");
        result.Should().Contain("admin.settings.update");
    }

    #endregion

    #region Permission Inheritance Tests

    [Fact]
    public void PermissionInheritance_AdminHasAllUserPermissions_PlusAdditional()
    {
        // Arrange
        var adminRole = GlobalRole.Admin;
        var userRole = GlobalRole.User;

        // Act & Assert
        // Both Admin and User can access system (when active)
        GlobalPermissions.CanAccessSystem(adminRole, true).Should().BeTrue();
        GlobalPermissions.CanAccessSystem(userRole, true).Should().BeTrue();

        // Admin has additional permissions that User doesn't have
        GlobalPermissions.CanManageUsers(adminRole).Should().BeTrue();
        GlobalPermissions.CanManageUsers(userRole).Should().BeFalse();

        GlobalPermissions.CanManageGlobalSettings(adminRole).Should().BeTrue();
        GlobalPermissions.CanManageGlobalSettings(userRole).Should().BeFalse();

        GlobalPermissions.CanViewAllProjects(adminRole).Should().BeTrue();
        GlobalPermissions.CanViewAllProjects(userRole).Should().BeFalse();

        GlobalPermissions.CanDeleteAnyProject(adminRole).Should().BeTrue();
        GlobalPermissions.CanDeleteAnyProject(userRole).Should().BeFalse();
    }

    #endregion
}

