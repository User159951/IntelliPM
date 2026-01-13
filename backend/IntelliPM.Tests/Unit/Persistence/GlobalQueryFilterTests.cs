using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Interfaces;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaskAsync = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Unit.Persistence;

/// <summary>
/// Unit tests to verify EF Core global query filters are applied to ITenantEntity types.
/// </summary>
public class GlobalQueryFilterTests
{
    [Fact]
    public void AppDbContext_ShouldHaveGlobalQueryFilter_ForITenantEntityTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Act - Check if query filter is configured
        var model = dbContext.Model;
        var entityTypes = model.GetEntityTypes()
            .Where(e => typeof(ITenantEntity).IsAssignableFrom(e.ClrType))
            .ToList();

        // Assert - All ITenantEntity types should have a query filter (either directly or inherited from base type)
        // In EF Core, derived types inherit query filters from their base types automatically
        foreach (var entityType in entityTypes)
        {
            var queryFilter = GetQueryFilterIncludingInherited(entityType);
            queryFilter.Should().NotBeNull(
                $"Entity type {entityType.ClrType.Name} implements ITenantEntity but has no query filter (including inherited)");
        }
    }

    /// <summary>
    /// Gets the query filter for an entity type, including checking for inherited filters from base types.
    /// EF Core inheritance hierarchies inherit query filters from their root type.
    /// </summary>
    private static System.Linq.Expressions.LambdaExpression? GetQueryFilterIncludingInherited(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType)
    {
        // Check this type first
        var filter = entityType.GetQueryFilter();
        if (filter != null)
        {
            return filter;
        }

        // Check base types (for inheritance hierarchies)
        var baseType = entityType.BaseType;
        while (baseType != null)
        {
            filter = baseType.GetQueryFilter();
            if (filter != null)
            {
                return filter;
            }
            baseType = baseType.BaseType;
        }

        return null;
    }

    [Fact]
    public async TaskAsync QueryFilter_ShouldFilterByOrganizationId_WhenCurrentOrganizationIdIsSet()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create test data
        var org1 = new Organization { Id = 1, Name = "Org1", CreatedAt = DateTimeOffset.UtcNow };
        var org2 = new Organization { Id = 2, Name = "Org2", CreatedAt = DateTimeOffset.UtcNow };
        dbContext.Organizations.AddRange(org1, org2);
        await dbContext.SaveChangesAsync();

        // Create projects for different organizations
        var project1 = new Project
        {
            Id = 1,
            Name = "Project1",
            OrganizationId = 1,
            OwnerId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var project2 = new Project
        {
            Id = 2,
            Name = "Project2",
            OrganizationId = 2,
            OwnerId = 2,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Bypass filter to insert data
        dbContext.BypassTenantFilter = true;
        dbContext.Projects.AddRange(project1, project2);
        await dbContext.SaveChangesAsync();
        dbContext.BypassTenantFilter = false;

        // Act - Set organization context and query
        dbContext.CurrentOrganizationId = 1;
        var projects = await dbContext.Projects.ToListAsync();

        // Assert - Should only return projects for organization 1
        projects.Should().HaveCount(1);
        projects[0].OrganizationId.Should().Be(1);
        projects[0].Name.Should().Be("Project1");
    }

    [Fact]
    public async TaskAsync QueryFilter_ShouldReturnEmpty_WhenCurrentOrganizationIdIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create test data
        var org1 = new Organization { Id = 1, Name = "Org1", CreatedAt = DateTimeOffset.UtcNow };
        dbContext.Organizations.Add(org1);
        await dbContext.SaveChangesAsync();

        var project1 = new Project
        {
            Id = 1,
            Name = "Project1",
            OrganizationId = 1,
            OwnerId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Bypass filter to insert data
        dbContext.BypassTenantFilter = true;
        dbContext.Projects.Add(project1);
        await dbContext.SaveChangesAsync();
        dbContext.BypassTenantFilter = false;

        // Act - Don't set organization context (null)
        dbContext.CurrentOrganizationId = null;
        var projects = await dbContext.Projects.ToListAsync();

        // Assert - Should return empty (filter excludes all when CurrentOrganizationId is null)
        projects.Should().BeEmpty();
    }

    [Fact]
    public async TaskAsync QueryFilter_ShouldBypass_WhenBypassTenantFilterIsTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create test data
        var org1 = new Organization { Id = 1, Name = "Org1", CreatedAt = DateTimeOffset.UtcNow };
        var org2 = new Organization { Id = 2, Name = "Org2", CreatedAt = DateTimeOffset.UtcNow };
        dbContext.Organizations.AddRange(org1, org2);
        await dbContext.SaveChangesAsync();

        var project1 = new Project
        {
            Id = 1,
            Name = "Project1",
            OrganizationId = 1,
            OwnerId = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var project2 = new Project
        {
            Id = 2,
            Name = "Project2",
            OrganizationId = 2,
            OwnerId = 2,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Bypass filter to insert data
        dbContext.BypassTenantFilter = true;
        dbContext.Projects.AddRange(project1, project2);
        await dbContext.SaveChangesAsync();

        // Act - Bypass filter and query
        dbContext.BypassTenantFilter = true;
        dbContext.CurrentOrganizationId = 1; // Set to org1, but bypass should return all
        var projects = await dbContext.Projects.ToListAsync();

        // Assert - Should return all projects when bypass is enabled
        projects.Should().HaveCount(2);
    }
}
