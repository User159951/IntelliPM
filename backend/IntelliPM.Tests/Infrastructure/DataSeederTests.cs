using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace IntelliPM.Tests.Infrastructure;

public class DataSeederTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Create a minimal service provider for AppDbContext constructor
        var serviceProvider = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .BuildServiceProvider();

        return new AppDbContext(options, serviceProvider);
    }

    private ILogger CreateLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<DataSeeder>();
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedDevelopmentAdminUserAsync_CreatesAdminInDevelopment()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var passwordHasher = new PasswordHasher();
        var logger = CreateLogger();

        // Act
        await DataSeeder.SeedDevelopmentAdminUserAsync(context, passwordHasher, logger, isDevelopment: true);

        // Assert
        Assert.True(await context.Organizations.AnyAsync(), "At least one organization should exist");
        Assert.True(await context.Users.AnyAsync(u => u.GlobalRole == GlobalRole.Admin), "Admin user should be created");
        
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.GlobalRole == GlobalRole.Admin);
        Assert.NotNull(adminUser);
        Assert.Equal("admin", adminUser.Username);
        Assert.Equal("admin@local", adminUser.Email);
        Assert.Equal("Dev", adminUser.FirstName);
        Assert.Equal("Admin", adminUser.LastName);
        Assert.True(adminUser.IsActive);
        Assert.True(adminUser.OrganizationId > 0);
        
        // Verify password works
        Assert.True(passwordHasher.VerifyPassword("Admin123!", adminUser.PasswordHash, adminUser.PasswordSalt), "Password should be valid");
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedDevelopmentAdminUserAsync_DoesNotRunOutsideDevelopment()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var passwordHasher = new PasswordHasher();
        var logger = CreateLogger();

        // Act
        await DataSeeder.SeedDevelopmentAdminUserAsync(context, passwordHasher, logger, isDevelopment: false);

        // Assert
        Assert.False(await context.Users.AnyAsync(u => u.GlobalRole == GlobalRole.Admin), "No admin user should be created in production");
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedDevelopmentAdminUserAsync_DoesNotDuplicateAdmin()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var passwordHasher = new PasswordHasher();
        var logger = CreateLogger();

        // Pre-create an admin user
        var organization = new Organization
        {
            Name = "Test Organization",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var existingAdmin = new User
        {
            Username = "existingadmin",
            Email = "existingadmin@test.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Existing",
            LastName = "Admin",
            GlobalRole = GlobalRole.Admin,
            IsActive = true,
            OrganizationId = organization.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Users.Add(existingAdmin);
        await context.SaveChangesAsync();

        var initialAdminCount = await context.Users.CountAsync(u => u.GlobalRole == GlobalRole.Admin);

        // Act
        await DataSeeder.SeedDevelopmentAdminUserAsync(context, passwordHasher, logger, isDevelopment: true);

        // Assert
        var finalAdminCount = await context.Users.CountAsync(u => u.GlobalRole == GlobalRole.Admin);
        Assert.Equal(initialAdminCount, finalAdminCount);
        Assert.Equal(1, finalAdminCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedDevelopmentAdminUserAsync_CreatesOrganizationIfNoneExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var passwordHasher = new PasswordHasher();
        var logger = CreateLogger();

        // Ensure no organizations exist
        Assert.False(await context.Organizations.AnyAsync());

        // Act
        await DataSeeder.SeedDevelopmentAdminUserAsync(context, passwordHasher, logger, isDevelopment: true);

        // Assert
        Assert.True(await context.Organizations.AnyAsync(), "Organization should be created");
        var organization = await context.Organizations.FirstOrDefaultAsync();
        Assert.NotNull(organization);
        Assert.Equal("Dev Organization", organization.Name);
        
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.GlobalRole == GlobalRole.Admin);
        Assert.NotNull(adminUser);
        Assert.Equal(organization.Id, adminUser.OrganizationId);
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedDevelopmentAdminUserAsync_UsesExistingOrganization()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var passwordHasher = new PasswordHasher();
        var logger = CreateLogger();

        // Pre-create an organization
        var existingOrg = new Organization
        {
            Name = "Existing Organization",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Organizations.Add(existingOrg);
        await context.SaveChangesAsync();

        // Act
        await DataSeeder.SeedDevelopmentAdminUserAsync(context, passwordHasher, logger, isDevelopment: true);

        // Assert
        var orgCount = await context.Organizations.CountAsync();
        Assert.Equal(1, orgCount); // Should not create a new organization
        
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.GlobalRole == GlobalRole.Admin);
        Assert.NotNull(adminUser);
        Assert.Equal(existingOrg.Id, adminUser.OrganizationId);
    }
}

