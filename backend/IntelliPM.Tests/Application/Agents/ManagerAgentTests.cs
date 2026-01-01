using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Tests.Application.Agents.TestHelpers;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Tests.Application.Agents;

public class ManagerAgentTests
{
    [Fact]
    public async Task RunAsync_ShouldReturnValidOutput_WhenGivenValidProjectId()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Manager context: Project milestones and key decisions from previous weeks.");

        var validJson = @"{
  ""executiveSummary"": ""This week showed significant progress. The team completed 85% of planned tasks. Key highlights include successful deployment of authentication module."",
  ""keyDecisions"": [""Decision 1"", ""Decision 2""],
  ""highlights"": [""Highlight 1"", ""Highlight 2""],
  ""confidence"": 0.88
}";
        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validJson);

        var parser = AgentParserTestHelper.CreateRealParser();
        var mockLogger = new Mock<ILogger<ManagerAgent>>();
        var agent = new ManagerAgent(mockLlmClient.Object, mockVectorStore.Object, parser, mockLogger.Object);
        var projectId = 1;
        var kpis = new Dictionary<string, object>
        {
            { "CompletedSprints", 5 },
            { "TotalTasks", 120 },
            { "CompletedTasks", 102 },
            { "CompletionRate", 85.0m }
        };
        var changes = "Task ABC-123: Status changed to Done\nTask ABC-124: Assigned to developer";
        var highlights = "Authentication feature deployed\nPerformance improved by 25%";

        // Act
        var result = await agent.RunAsync(projectId, kpis, changes, highlights, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ExecutiveSummary.Should().NotBeNullOrEmpty();
        result.KeyDecisionsNeeded.Should().NotBeNull();
        result.KeyDecisionsNeeded.Should().HaveCountGreaterThan(0);
        result.Highlights.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0);
        result.Confidence.Should().BeLessOrEqualTo(1);

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            projectId, "manager", 5, It.IsAny<CancellationToken>()), Times.Once);
        
        mockLlmClient.Verify(l => l.GenerateTextAsync(
            It.Is<string>(p => p.Contains("KPIs") && p.Contains("CHANGES THIS WEEK") && p.Contains("HIGHLIGHTS")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldIncludeRAGContext_WhenExecuted()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();
        var expectedContext = "Historical executive summaries: Last month showed strong delivery momentum.";

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContext);

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Executive summary generated.");

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<ManagerAgent>>();
        var agent = new ManagerAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act
        await agent.RunAsync(1, new Dictionary<string, object>(), "", "", CancellationToken.None);

        // Assert
        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            1, "manager", 5, It.IsAny<CancellationToken>()), Times.Once);
        
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
            .ThrowsAsync(new InvalidOperationException("VectorStore connection failed"));

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<ManagerAgent>>();
        var agent = new ManagerAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await agent.RunAsync(1, new Dictionary<string, object>(), "", "", CancellationToken.None));

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
            .ThrowsAsync(new TaskCanceledException("LLM request exceeded timeout"));

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<ManagerAgent>>();
        var agent = new ManagerAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await agent.RunAsync(1, new Dictionary<string, object>(), "", "", CancellationToken.None));

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldSerializeKPIs_WhenBuildingPrompt()
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
        var mockLogger = new Mock<ILogger<ManagerAgent>>();
        var agent = new ManagerAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);
        var kpis = new Dictionary<string, object>
        {
            { "CompletedSprints", 10 },
            { "Velocity", 45.5m }
        };

        // Act
        await agent.RunAsync(1, kpis, "Changes", "Highlights", CancellationToken.None);

        // Assert
        capturedPrompt.Should().NotBeNull();
        capturedPrompt.Should().Contain("KPIs");
        capturedPrompt.Should().Contain("CompletedSprints");
        capturedPrompt.Should().Contain("10");
        capturedPrompt.Should().Contain("CHANGES THIS WEEK");
        capturedPrompt.Should().Contain("Changes");
        capturedPrompt.Should().Contain("HIGHLIGHTS");
        capturedPrompt.Should().Contain("Highlights");
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

        var validJson = @"{
  ""executiveSummary"": ""Executive summary text"",
  ""keyDecisions"": [""Decision 1"", ""Decision 2""],
  ""highlights"": [""Test highlight"", ""Highlight 2""],
  ""confidence"": 0.88
}";
        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validJson);

        var parser = AgentParserTestHelper.CreateRealParser();
        var mockLogger = new Mock<ILogger<ManagerAgent>>();
        var agent = new ManagerAgent(mockLlmClient.Object, mockVectorStore.Object, parser, mockLogger.Object);
        var highlights = "Test highlight";

        // Act
        var result = await agent.RunAsync(1, new Dictionary<string, object>(), "", highlights, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ExecutiveSummary.Should().Be("Executive summary text");
        result.KeyDecisionsNeeded.Should().NotBeNull();
        result.KeyDecisionsNeeded.Should().HaveCount(2);
        result.Highlights.Should().NotBeNull();
        result.Highlights.Should().Contain(highlights);
        result.Confidence.Should().Be(0.88m);
    }
}

