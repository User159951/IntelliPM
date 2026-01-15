using IntelliPM.Application.Services;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace IntelliPM.Tests.Unit.Services;

public class LanguageServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<LanguageService>> _loggerMock;
    private readonly LanguageService _service;

    public LanguageServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<LanguageService>>();
        _service = new LanguageService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserLanguageAsync_UserHasPreference_ReturnsUserPreference()
    {
        // Arrange
        var organization = new IntelliPM.Domain.Entities.Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Organizations.Add(organization);

        var user = new IntelliPM.Domain.Entities.User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = 1,
            PreferredLanguage = "fr",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserLanguageAsync(1, 1);

        // Assert
        Assert.Equal("fr", result);
    }

    [Fact]
    public async Task GetUserLanguageAsync_NoUserPreference_UsesOrganizationDefault()
    {
        // Arrange
        var organization = new IntelliPM.Domain.Entities.Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            DefaultLanguage = "ar",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Organizations.Add(organization);

        var user = new IntelliPM.Domain.Entities.User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = 1,
            PreferredLanguage = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserLanguageAsync(1, 1);

        // Assert
        Assert.Equal("ar", result);
    }

    [Fact]
    public async Task GetUserLanguageAsync_NoPreference_UsesBrowserLanguage()
    {
        // Arrange
        var organization = new IntelliPM.Domain.Entities.Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Organizations.Add(organization);

        var user = new IntelliPM.Domain.Entities.User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = 1,
            PreferredLanguage = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserLanguageAsync(1, 1, "fr-FR,fr;q=0.9");

        // Assert
        Assert.Equal("fr", result);
    }

    [Fact]
    public async Task GetUserLanguageAsync_NoPreference_DefaultsToEnglish()
    {
        // Arrange
        var organization = new IntelliPM.Domain.Entities.Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Organizations.Add(organization);

        var user = new IntelliPM.Domain.Entities.User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = 1,
            PreferredLanguage = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserLanguageAsync(1, 1, "de-DE,de;q=0.9");

        // Assert
        Assert.Equal("en", result);
    }

    [Fact]
    public async Task UpdateUserLanguageAsync_ValidLanguage_UpdatesUserPreference()
    {
        // Arrange
        var organization = new IntelliPM.Domain.Entities.Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Organizations.Add(organization);

        var user = new IntelliPM.Domain.Entities.User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = 1,
            PreferredLanguage = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _service.UpdateUserLanguageAsync(1, "ar");

        // Assert
        var updatedUser = await _context.Users.FindAsync(1);
        Assert.NotNull(updatedUser);
        Assert.Equal("ar", updatedUser.PreferredLanguage);
    }

    [Fact]
    public async Task UpdateUserLanguageAsync_UnsupportedLanguage_ThrowsArgumentException()
    {
        // Arrange
        var organization = new IntelliPM.Domain.Entities.Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Organizations.Add(organization);

        var user = new IntelliPM.Domain.Entities.User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = 1,
            PreferredLanguage = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateUserLanguageAsync(1, "de"));
    }

    [Fact]
    public async Task UpdateUserLanguageAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateUserLanguageAsync(999, "en"));
    }

    [Fact]
    public async Task GetOrganizationLanguageAsync_OrganizationExists_ReturnsDefaultLanguage()
    {
        // Arrange
        var organization = new IntelliPM.Domain.Entities.Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            DefaultLanguage = "fr",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetOrganizationLanguageAsync(1);

        // Assert
        Assert.Equal("fr", result);
    }

    [Fact]
    public async Task GetOrganizationLanguageAsync_OrganizationNotFound_ReturnsNull()
    {
        // Act
        var result = await _service.GetOrganizationLanguageAsync(999);

        // Assert
        Assert.Null(result);
    }
}
