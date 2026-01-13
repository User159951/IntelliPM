using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;
using IntelliPM.Infrastructure.AI.Services;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Application.Services;

namespace IntelliPM.Tests.Unit.AI;

/// <summary>
/// Unit tests for SemanticKernelAgentService focusing on plugin registration
/// </summary>
public class SemanticKernelAgentServiceTests
{
    /// <summary>
    /// Verifies that creating multiple SemanticKernelAgentService instances with the same Kernel
    /// does not throw a duplicate key exception for TaskQualityPlugin.
    /// This simulates the scoped service + singleton Kernel DI pattern.
    /// </summary>
    [Fact]
    public void Constructor_WithSharedKernel_DoesNotThrowDuplicatePluginException()
    {
        // Arrange
        var kernel = Kernel.CreateBuilder().Build();
        var loggerMock = new Mock<ILogger<SemanticKernelAgentService>>();
        var dbContextMock = CreateMockDbContext();
        var configMock = CreateMockConfiguration();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var availabilityServiceMock = new Mock<IAIAvailabilityService>();
        var correlationIdServiceMock = new Mock<ICorrelationIdService>();
        
        currentUserServiceMock.Setup(x => x.GetUserId()).Returns(1);
        currentUserServiceMock.Setup(x => x.GetOrganizationId()).Returns(1);

        // Act & Assert - First instance should succeed
        var firstInstance = new SemanticKernelAgentService(
            kernel,
            loggerMock.Object,
            dbContextMock,
            configMock.Object,
            currentUserServiceMock.Object,
            availabilityServiceMock.Object,
            correlationIdServiceMock.Object);

        // Second instance with SAME kernel should NOT throw (simulates second request)
        var secondInstanceAction = () => new SemanticKernelAgentService(
            kernel,
            loggerMock.Object,
            dbContextMock,
            configMock.Object,
            currentUserServiceMock.Object,
            availabilityServiceMock.Object,
            correlationIdServiceMock.Object);

        secondInstanceAction.Should().NotThrow<ArgumentException>(
            "because AddPluginIfMissing should prevent duplicate registration");

        // Third instance should also not throw
        var thirdInstanceAction = () => new SemanticKernelAgentService(
            kernel,
            loggerMock.Object,
            dbContextMock,
            configMock.Object,
            currentUserServiceMock.Object,
            availabilityServiceMock.Object,
            correlationIdServiceMock.Object);

        thirdInstanceAction.Should().NotThrow<ArgumentException>();
    }

    /// <summary>
    /// Verifies that TaskQualityPlugin is registered exactly once even with multiple service instances.
    /// </summary>
    [Fact]
    public void Constructor_WithSharedKernel_RegistersTaskQualityPluginExactlyOnce()
    {
        // Arrange
        var kernel = Kernel.CreateBuilder().Build();
        var loggerMock = new Mock<ILogger<SemanticKernelAgentService>>();
        var dbContextMock = CreateMockDbContext();
        var configMock = CreateMockConfiguration();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var availabilityServiceMock = new Mock<IAIAvailabilityService>();
        var correlationIdServiceMock = new Mock<ICorrelationIdService>();
        
        currentUserServiceMock.Setup(x => x.GetUserId()).Returns(1);
        currentUserServiceMock.Setup(x => x.GetOrganizationId()).Returns(1);

        // Act - Create multiple instances
        _ = new SemanticKernelAgentService(kernel, loggerMock.Object, dbContextMock, configMock.Object,
            currentUserServiceMock.Object, availabilityServiceMock.Object, correlationIdServiceMock.Object);
        _ = new SemanticKernelAgentService(kernel, loggerMock.Object, dbContextMock, configMock.Object,
            currentUserServiceMock.Object, availabilityServiceMock.Object, correlationIdServiceMock.Object);
        _ = new SemanticKernelAgentService(kernel, loggerMock.Object, dbContextMock, configMock.Object,
            currentUserServiceMock.Object, availabilityServiceMock.Object, correlationIdServiceMock.Object);

        // Assert
        var taskQualityPluginCount = kernel.Plugins
            .Count(p => string.Equals(p.Name, "TaskQualityPlugin", StringComparison.OrdinalIgnoreCase));
        
        taskQualityPluginCount.Should().Be(1, 
            "TaskQualityPlugin should be registered exactly once regardless of how many service instances are created");
    }

    /// <summary>
    /// Verifies that the logger receives the correct message about plugin registration status.
    /// </summary>
    [Fact]
    public void Constructor_LogsPluginRegistrationStatus()
    {
        // Arrange
        var kernel = Kernel.CreateBuilder().Build();
        var loggerMock = new Mock<ILogger<SemanticKernelAgentService>>();
        var dbContextMock = CreateMockDbContext();
        var configMock = CreateMockConfiguration();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var availabilityServiceMock = new Mock<IAIAvailabilityService>();
        var correlationIdServiceMock = new Mock<ICorrelationIdService>();
        
        currentUserServiceMock.Setup(x => x.GetUserId()).Returns(1);
        currentUserServiceMock.Setup(x => x.GetOrganizationId()).Returns(1);

        // Act - First instance
        _ = new SemanticKernelAgentService(kernel, loggerMock.Object, dbContextMock, configMock.Object,
            currentUserServiceMock.Object, availabilityServiceMock.Object, correlationIdServiceMock.Object);

        // Assert - First instance logs "added"
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("added")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "First instance should log that plugin was 'added'");

        // Act - Second instance
        _ = new SemanticKernelAgentService(kernel, loggerMock.Object, dbContextMock, configMock.Object,
            currentUserServiceMock.Object, availabilityServiceMock.Object, correlationIdServiceMock.Object);

        // Assert - Second instance logs "already present"
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already present")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Second instance should log that plugin was 'already present'");
    }

    private static AppDbContext CreateMockDbContext()
    {
        // Create a minimal mock - the constructor doesn't use DbContext directly
        // In a real scenario, you'd use InMemory provider or TestContainers
        var optionsMock = new Mock<Microsoft.EntityFrameworkCore.DbContextOptions<AppDbContext>>();
        
        // Return null for now since constructor doesn't access DbContext
        // This is acceptable for unit tests focused on plugin registration
        return null!;
    }

    private static Mock<IConfiguration> CreateMockConfiguration()
    {
        var configMock = new Mock<IConfiguration>();
        
        // Setup Agent:TimeoutSeconds
        configMock.Setup(x => x.GetSection("Agent:TimeoutSeconds").Value).Returns("60");
        configMock.Setup(x => x["Agent:TimeoutSeconds"]).Returns("60");
        
        return configMock;
    }
}
