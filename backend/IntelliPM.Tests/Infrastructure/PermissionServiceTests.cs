using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Services;
using TaskEntity = IntelliPM.Domain.Entities.Task;

namespace IntelliPM.Tests.Infrastructure;

public class PermissionServiceTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;

    public PermissionServiceTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        
        _permissionService = new PermissionService(_context, _cacheServiceMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserPermissionsAsync_AdminUser_ReturnsAllPermissions()
    {
        // Arrange
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@test.com",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new[]
        {
            new Permission { Id = 1, Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 2, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 3, Name = "admin.access", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = new[]
        {
            new RolePermission { Id = 1, Role = GlobalRole.Admin, PermissionId = 1, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Id = 2, Role = GlobalRole.Admin, PermissionId = 2, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Id = 3, Role = GlobalRole.Admin, PermissionId = 3, CreatedAt = DateTimeOffset.UtcNow }
        };

        _context.Users.Add(adminUser);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        // Mock cache to return null (cache miss)
        _cacheServiceMock.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(adminUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain("projects.view");
        result.Should().Contain("projects.create");
        result.Should().Contain("admin.access");

        // Verify cache was set
        _cacheServiceMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserPermissionsAsync_RegularUser_ReturnsLimitedPermissions()
    {
        // Arrange
        var regularUser = new User
        {
            Id = 2,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permissions = new[]
        {
            new Permission { Id = 1, Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 2, Name = "projects.create", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 3, Name = "projects.delete", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow },
            new Permission { Id = 4, Name = "admin.access", Category = "Admin", CreatedAt = DateTimeOffset.UtcNow }
        };

        var rolePermissions = new[]
        {
            new RolePermission { Id = 1, Role = GlobalRole.User, PermissionId = 1, CreatedAt = DateTimeOffset.UtcNow },
            new RolePermission { Id = 2, Role = GlobalRole.User, PermissionId = 2, CreatedAt = DateTimeOffset.UtcNow }
        };

        _context.Users.Add(regularUser);
        _context.Permissions.AddRange(permissions);
        _context.RolePermissions.AddRange(rolePermissions);
        await _context.SaveChangesAsync();

        // Mock cache to return null (cache miss)
        _cacheServiceMock.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(regularUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain("projects.view");
        result.Should().Contain("projects.create");
        result.Should().NotContain("projects.delete");
        result.Should().NotContain("admin.access");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserPermissionsAsync_UserNotFound_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentUserId = 999;

        // Mock cache to return null (cache miss)
        _cacheServiceMock.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(nonExistentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserPermissionsAsync_UsesCache_WhenAvailable()
    {
        // Arrange
        var cachedPermissions = new List<string> { "projects.view", "projects.create" };
        _cacheServiceMock.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedPermissions);
        
        // Verify database was not queried (no SaveChangesAsync should be called)
        // This is implicit - if cache is hit, no DB query happens
    }

    [Fact]
    public async System.Threading.Tasks.Task HasPermissionAsync_UserHasPermission_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 3,
            Username = "testuser",
            Email = "test@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permission = new Permission { Id = 5, Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        var rolePermission = new RolePermission { Id = 5, Role = GlobalRole.User, PermissionId = 5, CreatedAt = DateTimeOffset.UtcNow };

        _context.Users.Add(user);
        _context.Permissions.Add(permission);
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        // Mock cache to return null (cache miss)
        _cacheServiceMock.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(user.Id, "projects.view");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task HasPermissionAsync_UserDoesNotHavePermission_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 4,
            Username = "testuser2",
            Email = "test2@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permission = new Permission { Id = 6, Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        var rolePermission = new RolePermission { Id = 6, Role = GlobalRole.User, PermissionId = 6, CreatedAt = DateTimeOffset.UtcNow };

        _context.Users.Add(user);
        _context.Permissions.Add(permission);
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        // Mock cache to return null (cache miss)
        _cacheServiceMock.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(user.Id, "admin.access");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task HasPermissionAsync_EmptyPermission_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            Username = "testuser3",
            Email = "test3@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionService.HasPermissionAsync(user.Id, "");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task HasPermissionAsync_PermissionCaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 6,
            Username = "testuser4",
            Email = "test4@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var permission = new Permission { Id = 7, Name = "projects.view", Category = "Projects", CreatedAt = DateTimeOffset.UtcNow };
        var rolePermission = new RolePermission { Id = 7, Role = GlobalRole.User, PermissionId = 7, CreatedAt = DateTimeOffset.UtcNow };

        _context.Users.Add(user);
        _context.Permissions.Add(permission);
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        // Mock cache to return null (cache miss)
        _cacheServiceMock.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string>?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(user.Id, "PROJECTS.VIEW");

        // Assert
        result.Should().BeTrue();
    }
}

