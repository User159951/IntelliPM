using IntelliPM.Application.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;

namespace IntelliPM.Tests.Unit.Services;

public class LanguageServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<LanguageService>> _loggerMock;
    private readonly LanguageService _service;

    public LanguageServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<LanguageService>>();
        _service = new LanguageService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserLanguageAsync_UserHasPreference_ReturnsUserPreference()
    {
        // Arrange
        var user = new User
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

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.Query())
            .Returns(new[] { user }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        // Act
        var result = await _service.GetUserLanguageAsync(1, 1);

        // Assert
        Assert.Equal("fr", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserLanguageAsync_NoUserPreference_UsesOrganizationDefault()
    {
        // Arrange
        var user = new User
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

        var organization = new Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            DefaultLanguage = "ar",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.Query())
            .Returns(new[] { user }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        var orgRepoMock = new Mock<IRepository<Organization>>();
        orgRepoMock.Setup(r => r.Query())
            .Returns(new[] { organization }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<Organization>())
            .Returns(orgRepoMock.Object);

        // Act
        var result = await _service.GetUserLanguageAsync(1, 1);

        // Assert
        Assert.Equal("ar", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserLanguageAsync_NoPreference_UsesBrowserLanguage()
    {
        // Arrange
        var user = new User
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

        var organization = new Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.Query())
            .Returns(new[] { user }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        var orgRepoMock = new Mock<IRepository<Organization>>();
        orgRepoMock.Setup(r => r.Query())
            .Returns(new[] { organization }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<Organization>())
            .Returns(orgRepoMock.Object);

        // Act
        var result = await _service.GetUserLanguageAsync(1, 1, "fr-FR,fr;q=0.9");

        // Assert
        Assert.Equal("fr", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetUserLanguageAsync_NoPreference_DefaultsToEnglish()
    {
        // Arrange
        var user = new User
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

        var organization = new Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.Query())
            .Returns(new[] { user }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        var orgRepoMock = new Mock<IRepository<Organization>>();
        orgRepoMock.Setup(r => r.Query())
            .Returns(new[] { organization }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<Organization>())
            .Returns(orgRepoMock.Object);

        // Act
        var result = await _service.GetUserLanguageAsync(1, 1, "de-DE,de;q=0.9");

        // Assert
        Assert.Equal("en", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateUserLanguageAsync_ValidLanguage_UpdatesUserPreference()
    {
        // Arrange
        var user = new User
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

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.Query())
            .Returns(new[] { user }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateUserLanguageAsync(1, "ar");

        // Assert
        Assert.Equal("ar", user.PreferredLanguage);
        userRepoMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateUserLanguageAsync_UnsupportedLanguage_ThrowsArgumentException()
    {
        // Arrange
        var user = new User
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

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.Query())
            .Returns(new[] { user }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateUserLanguageAsync(1, "de"));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateUserLanguageAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.Query())
            .Returns(Array.Empty<User>().AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateUserLanguageAsync(999, "en"));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOrganizationLanguageAsync_OrganizationExists_ReturnsDefaultLanguage()
    {
        // Arrange
        var organization = new Organization
        {
            Id = 1,
            Name = "Test Org",
            Code = "test",
            DefaultLanguage = "fr",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var orgRepoMock = new Mock<IRepository<Organization>>();
        orgRepoMock.Setup(r => r.Query())
            .Returns(new[] { organization }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<Organization>())
            .Returns(orgRepoMock.Object);

        // Act
        var result = await _service.GetOrganizationLanguageAsync(1);

        // Assert
        Assert.Equal("fr", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOrganizationLanguageAsync_OrganizationNotFound_ReturnsNull()
    {
        // Arrange
        var orgRepoMock = new Mock<IRepository<Organization>>();
        orgRepoMock.Setup(r => r.Query())
            .Returns(Array.Empty<Organization>().AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.Repository<Organization>())
            .Returns(orgRepoMock.Object);

        // Act
        var result = await _service.GetOrganizationLanguageAsync(999);

        // Assert
        Assert.Null(result);
    }
}
