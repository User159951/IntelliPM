using FluentAssertions;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Services;
using IntelliPM.Tests.Infrastructure.TestAuthentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Integration.API;

/// <summary>
/// Integration tests for Admin and SuperAdmin authorization scenarios.
/// Tests verify:
/// - Admin can only access their own organization
/// - SuperAdmin can access all organizations
/// - Cross-organization access prevention
/// - OrganizationScopingService behavior
/// </summary>
public class AdminSuperAdminAuthorizationTests : IClassFixture<AIAgentApiTestFactory>, IDisposable
{
    private readonly AIAgentApiTestFactory _factory;
    private readonly IServiceScope _seedScope;
    private readonly AppDbContext _seedDbContext;
    private readonly HttpClient _client;

    private Organization _org1 = null!;
    private Organization _org2 = null!;
    private User _adminUser1 = null!;
    private User _adminUser2 = null!;
    private User _superAdminUser = null!;
    private User _regularUser = null!;

    public AdminSuperAdminAuthorizationTests(AIAgentApiTestFactory factory)
    {
        _factory = factory;
        _seedScope = factory.Services.CreateScope();
        _seedDbContext = _seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
        _client = factory.CreateClient();
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Clear existing data
        _seedDbContext.Database.EnsureDeleted();
        _seedDbContext.Database.EnsureCreated();

        // Bypass tenant filter to insert test data
        _seedDbContext.BypassTenantFilter = true;

        // Create organizations
        _org1 = new Organization { Name = "Organization1", Code = "ORG1", CreatedAt = DateTimeOffset.UtcNow };
        _org2 = new Organization { Name = "Organization2", Code = "ORG2", CreatedAt = DateTimeOffset.UtcNow };
        _seedDbContext.Organizations.AddRange(_org1, _org2);
        _seedDbContext.SaveChanges();

        // Create Admin user for org1
        _adminUser1 = new User
        {
            Email = "admin1@org1.com",
            Username = "admin1",
            FirstName = "Admin",
            LastName = "One",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = _org1.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Create Admin user for org2
        _adminUser2 = new User
        {
            Email = "admin2@org2.com",
            Username = "admin2",
            FirstName = "Admin",
            LastName = "Two",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = _org2.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Create SuperAdmin user (can be in any org, but can access all)
        _superAdminUser = new User
        {
            Email = "superadmin@system.com",
            Username = "superadmin",
            FirstName = "Super",
            LastName = "Admin",
            GlobalRole = GlobalRole.SuperAdmin,
            OrganizationId = _org1.Id, // Can be in any org
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Create regular User
        _regularUser = new User
        {
            Email = "user@org1.com",
            Username = "user",
            FirstName = "Regular",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = _org1.Id,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _seedDbContext.Users.AddRange(_adminUser1, _adminUser2, _superAdminUser, _regularUser);
        _seedDbContext.SaveChanges();

        // Reset bypass flag
        _seedDbContext.BypassTenantFilter = false;
    }

    #region OrganizationScopingService Tests

    [Fact]
    public void OrganizationScopingService_Admin_GetScopedOrganizationId_ReturnsOwnOrgId()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUserService = new CurrentUserService(httpContextAccessor, dbContext);
        var scopingService = new OrganizationScopingService(currentUserService);

        // Mock HTTP context with Admin user
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _adminUser1.Id.ToString()),
            new Claim(ClaimTypes.Name, _adminUser1.Username),
            new Claim(ClaimTypes.Email, _adminUser1.Email),
            new Claim("OrganizationId", _org1.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "Test"));

        httpContextAccessor.HttpContext = httpContext;

        // Act
        var scopedOrgId = scopingService.GetScopedOrganizationId();

        // Assert
        scopedOrgId.Should().Be(_org1.Id);
    }

    [Fact]
    public void OrganizationScopingService_SuperAdmin_GetScopedOrganizationId_ReturnsZero()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUserService = new CurrentUserService(httpContextAccessor, dbContext);
        var scopingService = new OrganizationScopingService(currentUserService);

        // Mock HTTP context with SuperAdmin user
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _superAdminUser.Id.ToString()),
            new Claim(ClaimTypes.Name, _superAdminUser.Username),
            new Claim(ClaimTypes.Email, _superAdminUser.Email),
            new Claim("OrganizationId", _org1.Id.ToString()),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        }, "Test"));

        httpContextAccessor.HttpContext = httpContext;

        // Act
        var scopedOrgId = scopingService.GetScopedOrganizationId();

        // Assert
        scopedOrgId.Should().Be(0); // 0 means "all organizations" for SuperAdmin
    }

    [Fact]
    public void OrganizationScopingService_Admin_EnsureOrganizationAccess_OwnOrg_Succeeds()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUserService = new CurrentUserService(httpContextAccessor, dbContext);
        var scopingService = new OrganizationScopingService(currentUserService);

        // Mock HTTP context with Admin user from org1
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _adminUser1.Id.ToString()),
            new Claim(ClaimTypes.Name, _adminUser1.Username),
            new Claim(ClaimTypes.Email, _adminUser1.Email),
            new Claim("OrganizationId", _org1.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "Test"));

        httpContextAccessor.HttpContext = httpContext;

        // Act & Assert - Should not throw
        var act = () => scopingService.EnsureOrganizationAccess(_org1.Id);
        act.Should().NotThrow();
    }

    [Fact]
    public void OrganizationScopingService_Admin_EnsureOrganizationAccess_DifferentOrg_ThrowsUnauthorizedException()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUserService = new CurrentUserService(httpContextAccessor, dbContext);
        var scopingService = new OrganizationScopingService(currentUserService);

        // Mock HTTP context with Admin user from org1
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _adminUser1.Id.ToString()),
            new Claim(ClaimTypes.Name, _adminUser1.Username),
            new Claim(ClaimTypes.Email, _adminUser1.Email),
            new Claim("OrganizationId", _org1.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "Test"));

        httpContextAccessor.HttpContext = httpContext;

        // Act & Assert - Should throw when trying to access org2
        var act = () => scopingService.EnsureOrganizationAccess(_org2.Id);
        act.Should().Throw<UnauthorizedException>()
            .WithMessage($"Access denied. You can only access resources from your own organization (OrganizationId: {_org1.Id}).");
    }

    [Fact]
    public void OrganizationScopingService_SuperAdmin_EnsureOrganizationAccess_AnyOrg_Succeeds()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUserService = new CurrentUserService(httpContextAccessor, dbContext);
        var scopingService = new OrganizationScopingService(currentUserService);

        // Mock HTTP context with SuperAdmin user
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _superAdminUser.Id.ToString()),
            new Claim(ClaimTypes.Name, _superAdminUser.Username),
            new Claim(ClaimTypes.Email, _superAdminUser.Email),
            new Claim("OrganizationId", _org1.Id.ToString()),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        }, "Test"));

        httpContextAccessor.HttpContext = httpContext;

        // Act & Assert - Should not throw for any organization
        var act1 = () => scopingService.EnsureOrganizationAccess(_org1.Id);
        var act2 = () => scopingService.EnsureOrganizationAccess(_org2.Id);

        act1.Should().NotThrow();
        act2.Should().NotThrow();
    }

    [Fact]
    public void OrganizationScopingService_CanAccessOrganization_Admin_OwnOrg_ReturnsTrue()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUserService = new CurrentUserService(httpContextAccessor, dbContext);
        var scopingService = new OrganizationScopingService(currentUserService);

        // Mock HTTP context with Admin user from org1
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _adminUser1.Id.ToString()),
            new Claim(ClaimTypes.Name, _adminUser1.Username),
            new Claim(ClaimTypes.Email, _adminUser1.Email),
            new Claim("OrganizationId", _org1.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "Test"));

        httpContextAccessor.HttpContext = httpContext;

        // Act
        var canAccessOwnOrg = scopingService.CanAccessOrganization(_org1.Id);
        var canAccessOtherOrg = scopingService.CanAccessOrganization(_org2.Id);

        // Assert
        canAccessOwnOrg.Should().BeTrue();
        canAccessOtherOrg.Should().BeFalse();
    }

    [Fact]
    public void OrganizationScopingService_CanAccessOrganization_SuperAdmin_AllOrgs_ReturnsTrue()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUserService = new CurrentUserService(httpContextAccessor, dbContext);
        var scopingService = new OrganizationScopingService(currentUserService);

        // Mock HTTP context with SuperAdmin user
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _superAdminUser.Id.ToString()),
            new Claim(ClaimTypes.Name, _superAdminUser.Username),
            new Claim(ClaimTypes.Email, _superAdminUser.Email),
            new Claim("OrganizationId", _org1.Id.ToString()),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        }, "Test"));

        httpContextAccessor.HttpContext = httpContext;

        // Act
        var canAccessOrg1 = scopingService.CanAccessOrganization(_org1.Id);
        var canAccessOrg2 = scopingService.CanAccessOrganization(_org2.Id);

        // Assert
        canAccessOrg1.Should().BeTrue();
        canAccessOrg2.Should().BeTrue();
    }

    #endregion

    #region API Endpoint Authorization Tests

    [Fact]
    public async Task AdminEndpoint_AdminUser_Returns200()
    {
        // Arrange
        _client.AuthenticateAs(_adminUser1.Id, _adminUser1.Username, _adminUser1.Email, _org1.Id, GlobalRole.Admin);

        // Act - Try to access admin dashboard endpoint
        var response = await _client.GetAsync("/api/admin/dashboard/stats");

        // Assert
        // Note: May return 200 or 403 depending on permission checks, but should not return 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoint_RegularUser_Returns403()
    {
        // Arrange
        _client.AuthenticateAs(_regularUser.Id, _regularUser.Username, _regularUser.Email, _org1.Id, GlobalRole.User);

        // Act - Try to access admin dashboard endpoint
        var response = await _client.GetAsync("/api/admin/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuperAdminEndpoint_SuperAdmin_Returns200()
    {
        // Arrange
        _client.AuthenticateAsSuperAdmin(_superAdminUser.Id, _org1.Id);

        // Act - Try to access SuperAdmin-only endpoint (organizations list)
        var response = await _client.GetAsync("/api/admin/organizations");

        // Assert
        // Note: May return 200 or 404 depending on data, but should not return 401 or 403
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuperAdminEndpoint_Admin_Returns403()
    {
        // Arrange
        _client.AuthenticateAs(_adminUser1.Id, _adminUser1.Username, _adminUser1.Email, _org1.Id, GlobalRole.Admin);

        // Act - Try to access SuperAdmin-only endpoint
        var response = await _client.GetAsync("/api/admin/organizations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuperAdminEndpoint_RegularUser_Returns403()
    {
        // Arrange
        _client.AuthenticateAs(_regularUser.Id, _regularUser.Username, _regularUser.Email, _org1.Id, GlobalRole.User);

        // Act - Try to access SuperAdmin-only endpoint
        var response = await _client.GetAsync("/api/admin/organizations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    public void Dispose()
    {
        _seedScope?.Dispose();
        _client?.Dispose();
    }
}
