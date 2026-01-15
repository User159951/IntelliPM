using FluentAssertions;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IntelliPM.Tests.Unit.Services;

/// <summary>
/// Unit tests for AiGovernanceService
/// </summary>
public class AiGovernanceServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAIDecisionLogger> _decisionLoggerMock;
    private readonly Mock<IAIAvailabilityService> _availabilityServiceMock;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<AiGovernanceService>> _loggerMock;
    private readonly AiGovernanceService _service;

    public AiGovernanceServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _decisionLoggerMock = new Mock<IAIDecisionLogger>();
        _availabilityServiceMock = new Mock<IAIAvailabilityService>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<AiGovernanceService>>();

        _service = new AiGovernanceService(
            _unitOfWorkMock.Object,
            _cache,
            _loggerMock.Object,
            _decisionLoggerMock.Object,
            _availabilityServiceMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task ValidateAIExecutionAsync_WhenGlobalAIDisabled_ThrowsAIDisabledException()
    {
        // Arrange
        var organizationId = 1;
        var quotaType = "Requests";

        // Mock global AI disabled
        var globalSettingRepoMock = new Mock<IRepository<GlobalSetting>>();
        var globalSetting = new GlobalSetting
        {
            Key = "AI.Enabled",
            Value = "false"
        };
        globalSettingRepoMock.Setup(r => r.Query())
            .Returns(new[] { globalSetting }.AsQueryable());
        _unitOfWorkMock.Setup(u => u.Repository<GlobalSetting>())
            .Returns(globalSettingRepoMock.Object);

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.ValidateAIExecutionAsync(organizationId, quotaType);

        // Assert
        await act.Should().ThrowAsync<AIDisabledException>()
            .WithMessage("*system-wide*");
    }

    [Fact]
    public async System.Threading.Tasks.Task ValidateAIExecutionAsync_WhenOrgAIDisabled_ThrowsAIDisabledException()
    {
        // Arrange
        var organizationId = 1;
        var quotaType = "Requests";

        // Mock global AI enabled
        var globalSettingRepoMock = new Mock<IRepository<GlobalSetting>>();
        globalSettingRepoMock.Setup(r => r.Query())
            .Returns(Array.Empty<GlobalSetting>().AsQueryable());
        _unitOfWorkMock.Setup(u => u.Repository<GlobalSetting>())
            .Returns(globalSettingRepoMock.Object);

        // Mock organization AI disabled
        _availabilityServiceMock.Setup(s => s.IsAIEnabledForOrganization(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.ValidateAIExecutionAsync(organizationId, quotaType);

        // Assert
        await act.Should().ThrowAsync<AIDisabledException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task ValidateAIExecutionAsync_WhenQuotaExceeded_ThrowsAIQuotaExceededException()
    {
        // Arrange
        var organizationId = 1;
        var quotaType = "Requests";

        // Mock global AI enabled
        var globalSettingRepoMock = new Mock<IRepository<GlobalSetting>>();
        globalSettingRepoMock.Setup(r => r.Query())
            .Returns(Array.Empty<GlobalSetting>().AsQueryable());
        _unitOfWorkMock.Setup(u => u.Repository<GlobalSetting>())
            .Returns(globalSettingRepoMock.Object);

        // Mock organization AI enabled
        _availabilityServiceMock.Setup(s => s.IsAIEnabledForOrganization(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Mock quota exceeded
        _availabilityServiceMock.Setup(s => s.CheckQuotaAsync(organizationId, quotaType, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AIQuotaExceededException(
                "Quota exceeded",
                organizationId,
                quotaType,
                100,
                50,
                "Free"));

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.ValidateAIExecutionAsync(organizationId, quotaType);

        // Assert
        await act.Should().ThrowAsync<AIQuotaExceededException>();
    }

    [Fact]
    public async System.Threading.Tasks.Task ValidateAIExecutionAsync_WhenAllChecksPass_DoesNotThrow()
    {
        // Arrange
        var organizationId = 1;
        var quotaType = "Requests";

        // Mock global AI enabled
        var globalSettingRepoMock = new Mock<IRepository<GlobalSetting>>();
        globalSettingRepoMock.Setup(r => r.Query())
            .Returns(Array.Empty<GlobalSetting>().AsQueryable());
        _unitOfWorkMock.Setup(u => u.Repository<GlobalSetting>())
            .Returns(globalSettingRepoMock.Object);

        // Mock organization AI enabled
        _availabilityServiceMock.Setup(s => s.IsAIEnabledForOrganization(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Mock quota check passes
        _availabilityServiceMock.Setup(s => s.CheckQuotaAsync(organizationId, quotaType, It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        Func<System.Threading.Tasks.Task> act = async () => await _service.ValidateAIExecutionAsync(organizationId, quotaType);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async System.Threading.Tasks.Task IsGlobalAIEnabledAsync_WhenSettingDoesNotExist_ReturnsTrue()
    {
        // Arrange
        var globalSettingRepoMock = new Mock<IRepository<GlobalSetting>>();
        globalSettingRepoMock.Setup(r => r.Query())
            .Returns(Array.Empty<GlobalSetting>().AsQueryable());
        _unitOfWorkMock.Setup(u => u.Repository<GlobalSetting>())
            .Returns(globalSettingRepoMock.Object);

        // Act
        var result = await _service.IsGlobalAIEnabledAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task IsGlobalAIEnabledAsync_WhenSettingIsTrue_ReturnsTrue()
    {
        // Arrange
        var globalSettingRepoMock = new Mock<IRepository<GlobalSetting>>();
        var globalSetting = new GlobalSetting
        {
            Key = "AI.Enabled",
            Value = "true"
        };
        globalSettingRepoMock.Setup(r => r.Query())
            .Returns(new[] { globalSetting }.AsQueryable());
        _unitOfWorkMock.Setup(u => u.Repository<GlobalSetting>())
            .Returns(globalSettingRepoMock.Object);

        // Act
        var result = await _service.IsGlobalAIEnabledAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task IsGlobalAIEnabledAsync_WhenSettingIsFalse_ReturnsFalse()
    {
        // Arrange
        var globalSettingRepoMock = new Mock<IRepository<GlobalSetting>>();
        var globalSetting = new GlobalSetting
        {
            Key = "AI.Enabled",
            Value = "false"
        };
        globalSettingRepoMock.Setup(r => r.Query())
            .Returns(new[] { globalSetting }.AsQueryable());
        _unitOfWorkMock.Setup(u => u.Repository<GlobalSetting>())
            .Returns(globalSettingRepoMock.Object);

        // Act
        var result = await _service.IsGlobalAIEnabledAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task LogAIExecutionAsync_WhenCalled_ReturnsDecisionLogId()
    {
        // Arrange
        var organizationId = 1;
        var userId = 1;
        var decisionLogId = 123;

        _decisionLoggerMock.Setup(l => l.LogDecisionAsync(
            It.IsAny<string>(), // agentType
            It.IsAny<string>(), // decisionType
            It.IsAny<string>(), // reasoning
            It.IsAny<decimal>(), // confidenceScore
            It.IsAny<Dictionary<string, object>?>(), // metadata
            userId,
            organizationId,
            It.IsAny<int?>(), // projectId
            It.IsAny<string>(), // entityType
            It.IsAny<int?>(), // entityId
            It.IsAny<string?>(), // entityName
            It.IsAny<string?>(), // question
            It.IsAny<string?>(), // decision
            It.IsAny<string?>(), // inputData
            It.IsAny<string?>(), // outputData
            It.IsAny<string>(), // modelName
            It.IsAny<int>(), // tokensUsed
            It.IsAny<int>(), // promptTokens
            It.IsAny<int>(), // completionTokens
            It.IsAny<int>(), // executionTimeMs
            It.IsAny<bool>(), // isSuccess
            It.IsAny<string?>(), // errorMessage
            It.IsAny<string?>(), // correlationId
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(decisionLogId);

        // Mock entity name lookup
        var projectRepoMock = new Mock<IRepository<Project>>();
        var project = new Project { Id = 1, Name = "Test Project" };
        projectRepoMock.Setup(r => r.Query())
            .Returns(new[] { project }.AsQueryable());
        _unitOfWorkMock.Setup(u => u.Repository<Project>())
            .Returns(projectRepoMock.Object);

        // Act
        var result = await _service.LogAIExecutionAsync(
            organizationId,
            userId,
            "RiskDetection",
            "DeliveryAgent",
            "Project",
            1,
            new { test = "data" },
            "llama3.2:3b",
            100,
            50,
            50,
            "Decision outcome",
            0.85m,
            500,
            false,
            "correlation-id");

        // Assert
        result.Should().Be(decisionLogId);
    }
}
