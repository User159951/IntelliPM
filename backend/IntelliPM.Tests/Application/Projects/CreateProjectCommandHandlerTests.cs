using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Projects.Commands;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MockQueryable.Moq;
using DomainTask = IntelliPM.Domain.Entities.Task;

namespace IntelliPM.Tests.Application.Projects;

public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_WithValidData_CreatesProject()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockProjectRepo = new Mock<IRepository<Project>>();
        var mockSettingsRepo = new Mock<IRepository<GlobalSetting>>();
        var mockUserRepo = new Mock<IRepository<User>>();
        var mockMemberRepo = new Mock<IRepository<ProjectMember>>();
        var mockNotificationRepo = new Mock<IRepository<Notification>>();
        var mockActivityRepo = new Mock<IRepository<Activity>>();
        var mockCacheService = new Mock<ICacheService>();

        // Setup empty settings query (no restriction) - return null for FirstOrDefaultAsync
        var emptySettings = new List<GlobalSetting>();
        var emptySettingsMock = emptySettings.AsQueryable().BuildMock();
        mockSettingsRepo.Setup(r => r.Query()).Returns(emptySettingsMock);

        // Setup user query
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            GlobalRole = GlobalRole.Admin,
            OrganizationId = 1,
            IsActive = true
        };
        var usersMock = new List<User> { adminUser }.AsQueryable().BuildMock();
        mockUserRepo.Setup(r => r.Query()).Returns(usersMock);
        mockUserRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        mockUnitOfWork.Setup(u => u.Repository<Project>())
            .Returns(mockProjectRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<GlobalSetting>())
            .Returns(mockSettingsRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<User>())
            .Returns(mockUserRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<ProjectMember>())
            .Returns(mockMemberRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<Notification>())
            .Returns(mockNotificationRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<Activity>())
            .Returns(mockActivityRepo.Object);

        // Setup cache service
        mockCacheService.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        var handler = new CreateProjectCommandHandler(mockUnitOfWork.Object, mockCacheService.Object);

        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            14,
            1
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Project");
        result.Description.Should().Be("Test Description");
        result.Type.Should().Be("Scrum");

        mockProjectRepo.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_WhenSettingIsAdmin_NonAdminUserThrowsUnauthorizedException()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockProjectRepo = new Mock<IRepository<Project>>();
        var mockSettingsRepo = new Mock<IRepository<GlobalSetting>>();
        var mockUserRepo = new Mock<IRepository<User>>();
        var mockCacheService = new Mock<ICacheService>();

        // Setup settings with "Admin" restriction
        var setting = new GlobalSetting
        {
            Id = 1,
            Key = "ProjectCreation.AllowedRoles",
            Value = "Admin",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var settingsMock = new List<GlobalSetting> { setting }.AsQueryable().BuildMock();
        mockSettingsRepo.Setup(r => r.Query()).Returns(settingsMock);

        // Setup regular user (not admin)
        var regularUser = new User
        {
            Id = 2,
            Username = "user",
            Email = "user@test.com",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true
        };
        var usersMock = new List<User> { regularUser }.AsQueryable().BuildMock();
        mockUserRepo.Setup(r => r.Query()).Returns(usersMock);

        mockUnitOfWork.Setup(u => u.Repository<Project>())
            .Returns(mockProjectRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<GlobalSetting>())
            .Returns(mockSettingsRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<User>())
            .Returns(mockUserRepo.Object);

        var handler = new CreateProjectCommandHandler(mockUnitOfWork.Object, mockCacheService.Object);

        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            14,
            2 // Regular user ID
        );

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
        mockProjectRepo.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_WhenSettingIsAdminUser_NonAdminUserCanCreate()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockProjectRepo = new Mock<IRepository<Project>>();
        var mockSettingsRepo = new Mock<IRepository<GlobalSetting>>();
        var mockUserRepo = new Mock<IRepository<User>>();
        var mockMemberRepo = new Mock<IRepository<ProjectMember>>();
        var mockNotificationRepo = new Mock<IRepository<Notification>>();
        var mockActivityRepo = new Mock<IRepository<Activity>>();
        var mockCacheService = new Mock<ICacheService>();

        // Setup settings with "Admin,User" (all users allowed)
        var setting = new GlobalSetting
        {
            Id = 1,
            Key = "ProjectCreation.AllowedRoles",
            Value = "Admin,User",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var settingsMock = new List<GlobalSetting> { setting }.AsQueryable().BuildMock();
        mockSettingsRepo.Setup(r => r.Query()).Returns(settingsMock);

        // Setup regular user
        var regularUser = new User
        {
            Id = 2,
            Username = "user",
            Email = "user@test.com",
            FirstName = "Regular",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true
        };
        var usersMock = new List<User> { regularUser }.AsQueryable().BuildMock();
        mockUserRepo.Setup(r => r.Query()).Returns(usersMock);
        mockUserRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(regularUser);

        mockUnitOfWork.Setup(u => u.Repository<Project>())
            .Returns(mockProjectRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<GlobalSetting>())
            .Returns(mockSettingsRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<User>())
            .Returns(mockUserRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<ProjectMember>())
            .Returns(mockMemberRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<Notification>())
            .Returns(mockNotificationRepo.Object);
        mockUnitOfWork.Setup(u => u.Repository<Activity>())
            .Returns(mockActivityRepo.Object);

        // Setup cache service
        mockCacheService.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        var handler = new CreateProjectCommandHandler(mockUnitOfWork.Object, mockCacheService.Object);

        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            14,
            2 // Regular user ID
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Project");
        mockProjectRepo.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

