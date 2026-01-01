using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Tests.Application.Agents;

public class DeliveryAgentTests
{
    [Fact]
    public async Task RunAsync_ShouldReturnValidOutput_WhenGivenValidProjectId()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Delivery context: Previous sprint completed on time, team velocity stable.");

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Risk assessment: Project is on track. Current sprint shows good progress. Velocity trend is consistent.");

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<DeliveryAgent>>();
        var agent = new DeliveryAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);
        var projectId = 1;
        var sprintProgress = "Sprint 5: 12/20 tasks completed";
        var velocityTrend = new List<decimal> { 25.5m, 28.0m, 27.0m, 26.5m, 27.5m };
        var activeRisks = new List<string> { "High: Dependency on external API", "Medium: Resource availability" };

        // Act
        var result = await agent.RunAsync(projectId, sprintProgress, velocityTrend, activeRisks, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RiskAssessment.Should().NotBeNullOrEmpty();
        result.RecommendedActions.Should().NotBeNull();
        result.RecommendedActions.Should().HaveCountGreaterThan(0);
        result.Confidence.Should().BeGreaterThan(0);
        result.Confidence.Should().BeLessOrEqualTo(1);

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            projectId, "delivery", 5, It.IsAny<CancellationToken>()), Times.Once);
        
        mockLlmClient.Verify(l => l.GenerateTextAsync(
            It.Is<string>(p => p.Contains(sprintProgress) && p.Contains("VELOCITY TREND") && p.Contains("ACTIVE RISKS")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldIncludeRAGContext_WhenExecuted()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();
        var expectedContext = "Historical delivery data: Last quarter showed 95% on-time delivery rate.";

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContext);

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Risk analysis complete.");

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<DeliveryAgent>>();
        var agent = new DeliveryAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act
        await agent.RunAsync(1, "Sprint progress", new List<decimal>(), new List<string>(), CancellationToken.None);

        // Assert
        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            1, "delivery", 5, It.IsAny<CancellationToken>()), Times.Once);
        
        mockLlmClient.Verify(l => l.GenerateTextAsync(
            It.Is<string>(p => p.Contains("PROJECT CONTEXT") && p.Contains(expectedContext)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldHandleVectorStoreException_WhenVectorStoreFails()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("VectorStore unavailable"));

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<DeliveryAgent>>();
        var agent = new DeliveryAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await agent.RunAsync(1, "Sprint progress", new List<decimal>(), new List<string>(), CancellationToken.None));

        mockLlmClient.Verify(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_ShouldHandleLLMTimeout_WhenLLMClientTimesOut()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Context data");

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("LLM request timed out"));

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<DeliveryAgent>>();
        var agent = new DeliveryAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await agent.RunAsync(1, "Sprint progress", new List<decimal>(), new List<string>(), CancellationToken.None));

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldIncludeAllInputParameters_WhenBuildingPrompt()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();
        string? capturedPrompt = null;

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Context");

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((p, ct) =>
            {
                capturedPrompt = p;
                return Task.FromResult("Response");
            });

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<DeliveryAgent>>();
        var agent = new DeliveryAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);
        var sprintProgress = "Sprint 3: 15/18 tasks completed";
        var velocityTrend = new List<decimal> { 30.0m, 32.0m, 31.5m };
        var activeRisks = new List<string> { "High: Critical dependency delay" };

        // Act
        await agent.RunAsync(1, sprintProgress, velocityTrend, activeRisks, CancellationToken.None);

        // Assert
        capturedPrompt.Should().NotBeNull();
        capturedPrompt.Should().Contain(sprintProgress);
        capturedPrompt.Should().Contain("VELOCITY TREND");
        capturedPrompt.Should().Contain("30");
        capturedPrompt.Should().Contain("ACTIVE RISKS");
        capturedPrompt.Should().Contain("Critical dependency delay");
        capturedPrompt.Should().Contain("PROJECT CONTEXT");
    }

    [Fact]
    public async Task RunAsync_ShouldReturnStructuredOutput_WithCorrectProperties()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Context");

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Detailed risk assessment output");

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<DeliveryAgent>>();
        var agent = new DeliveryAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act
        var result = await agent.RunAsync(1, "Progress", new List<decimal>(), new List<string>(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RiskAssessment.Should().Be("Detailed risk assessment output");
        result.RecommendedActions.Should().NotBeNull();
        result.RecommendedActions.Should().HaveCount(3);
        result.Confidence.Should().Be(0.82m);
    }
}

