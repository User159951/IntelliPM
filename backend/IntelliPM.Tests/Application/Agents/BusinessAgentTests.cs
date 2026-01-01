using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Agents.DTOs;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Tests.Application.Agents.TestHelpers;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Tests.Application.Agents;

public class BusinessAgentTests
{
    [Fact]
    public async Task RunAsync_ShouldReturnValidOutput_WhenGivenValidProjectId()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();
        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<BusinessAgent>>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Business context: Market analysis and competitive positioning data.");

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Business Value Summary: The project delivered significant ROI. Customer satisfaction improved by 40%. Market advantage gained through innovative features.");


        var agent = new BusinessAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);
        var projectId = 1;
        var kpis = new Dictionary<string, object>
        {
            { "CompletedSprints", 12 },
            { "CompletedFeatures", 45 },
            { "TotalStoryPoints", 380 }
        };
        var completedFeatures = new List<string>
        {
            "User Authentication System",
            "Payment Processing Module",
            "Real-time Notifications"
        };
        var businessMetrics = new Dictionary<string, decimal>
        {
            { "Velocity", 32.5m },
            { "DefectRate", 0.08m },
            { "CustomerSatisfaction", 4.5m }
        };

        // Act
        var result = await agent.RunAsync(projectId, kpis, completedFeatures, businessMetrics, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ValueDeliverySummary.Should().NotBeNullOrEmpty();
        result.ValueMetrics.Should().NotBeNull();
        result.ValueMetrics.Should().HaveCountGreaterThan(0);
        result.BusinessHighlights.Should().NotBeNull();
        result.BusinessHighlights.Should().HaveCountGreaterThan(0);
        result.StrategicRecommendations.Should().NotBeNull();
        result.StrategicRecommendations.Should().HaveCountGreaterThan(0);
        result.Confidence.Should().BeGreaterThan(0);
        result.Confidence.Should().BeLessOrEqualTo(1);

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            projectId, "business", 5, It.IsAny<CancellationToken>()), Times.Once);
        
        mockLlmClient.Verify(l => l.GenerateTextAsync(
            It.Is<string>(p => p.Contains("KEY PERFORMANCE INDICATORS") && p.Contains("COMPLETED FEATURES") && p.Contains("BUSINESS METRICS")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldIncludeRAGContext_WhenExecuted()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();
        var expectedContext = "Historical business value data: Previous quarter showed 120% ROI.";

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContext);

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Business analysis complete.");

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<BusinessAgent>>();
        var agent = new BusinessAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act
        await agent.RunAsync(1, new Dictionary<string, object>(), new List<string>(), new Dictionary<string, decimal>(), CancellationToken.None);

        // Assert
        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            1, "business", 5, It.IsAny<CancellationToken>()), Times.Once);
        
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
            .ThrowsAsync(new InvalidOperationException("VectorStore query failed"));

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<BusinessAgent>>();
        var agent = new BusinessAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await agent.RunAsync(1, new Dictionary<string, object>(), new List<string>(), new Dictionary<string, decimal>(), CancellationToken.None));

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
            .ThrowsAsync(new TaskCanceledException("LLM service timeout"));

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<BusinessAgent>>();
        var agent = new BusinessAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await agent.RunAsync(1, new Dictionary<string, object>(), new List<string>(), new Dictionary<string, decimal>(), CancellationToken.None));

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldSerializeKPIsAndMetrics_WhenBuildingPrompt()
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
        var mockLogger = new Mock<ILogger<BusinessAgent>>();
        var agent = new BusinessAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);
        var kpis = new Dictionary<string, object>
        {
            { "CompletedSprints", 20 },
            { "Revenue", 500000m }
        };
        var completedFeatures = new List<string> { "Feature A", "Feature B" };
        var businessMetrics = new Dictionary<string, decimal>
        {
            { "ROI", 150.5m },
            { "CustomerSatisfaction", 4.8m }
        };

        // Act
        await agent.RunAsync(1, kpis, completedFeatures, businessMetrics, CancellationToken.None);

        // Assert
        capturedPrompt.Should().NotBeNull();
        capturedPrompt.Should().Contain("KEY PERFORMANCE INDICATORS");
        capturedPrompt.Should().Contain("CompletedSprints");
        capturedPrompt.Should().Contain("COMPLETED FEATURES");
        capturedPrompt.Should().Contain("Feature A");
        capturedPrompt.Should().Contain("BUSINESS METRICS");
        capturedPrompt.Should().Contain("ROI");
        capturedPrompt.Should().Contain("150.5");
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
  ""valueDeliverySummary"": ""Business value summary text"",
  ""valueMetrics"": {
    ""EstimatedROI"": 145.5,
    ""TimeToMarket"": 85.0,
    ""CustomerSatisfaction"": 4.2
  },
  ""businessHighlights"": [""Highlight 1"", ""Highlight 2"", ""Highlight 3""],
  ""strategicRecommendations"": [""Recommendation 1"", ""Recommendation 2"", ""Recommendation 3""],
  ""confidence"": 0.87
}";
        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validJson);

        var parser = AgentParserTestHelper.CreateRealParser();
        var mockLogger = new Mock<ILogger<BusinessAgent>>();
        var agent = new BusinessAgent(mockLlmClient.Object, mockVectorStore.Object, parser, mockLogger.Object);

        // Act
        var result = await agent.RunAsync(1, new Dictionary<string, object>(), new List<string>(), new Dictionary<string, decimal>(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ValueDeliverySummary.Should().Be("Business value summary text");
        result.ValueMetrics.Should().NotBeNull();
        result.ValueMetrics.Should().HaveCount(3);
        result.ValueMetrics.Should().ContainKey("EstimatedROI");
        result.ValueMetrics.Should().ContainKey("TimeToMarket");
        result.ValueMetrics.Should().ContainKey("CustomerSatisfaction");
        result.ValueMetrics["EstimatedROI"].Should().Be(145.5m);
        result.BusinessHighlights.Should().NotBeNull();
        result.BusinessHighlights.Should().HaveCount(3);
        result.StrategicRecommendations.Should().NotBeNull();
        result.StrategicRecommendations.Should().HaveCount(3);
        result.Confidence.Should().Be(0.87m);
    }

    [Fact]
    public async Task RunAsync_ShouldIncludeValueMetrics_WithCorrectValues()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Context");

        var validJson = @"{
  ""valueDeliverySummary"": ""Analysis summary"",
  ""valueMetrics"": {
    ""EstimatedROI"": 145.5,
    ""TimeToMarket"": 85.0,
    ""CustomerSatisfaction"": 4.2
  },
  ""businessHighlights"": [
    ""Delivered authentication feature ahead of schedule"",
    ""Reduced technical debt by 20%"",
    ""Improved system performance by 35%""
  ],
  ""strategicRecommendations"": [
    ""Focus next sprint on high-value customer-facing features"",
    ""Consider A/B testing for new UI components"",
    ""Recommendation 3""
  ],
  ""confidence"": 0.87
}";
        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validJson);

        var parser = AgentParserTestHelper.CreateRealParser();
        var mockLogger = new Mock<ILogger<BusinessAgent>>();
        var agent = new BusinessAgent(mockLlmClient.Object, mockVectorStore.Object, parser, mockLogger.Object);

        // Act
        var result = await agent.RunAsync(1, new Dictionary<string, object>(), new List<string>(), new Dictionary<string, decimal>(), CancellationToken.None);

        // Assert
        result.ValueMetrics.Should().ContainKey("EstimatedROI");
        result.ValueMetrics["EstimatedROI"].Should().Be(145.5m);
        result.ValueMetrics["TimeToMarket"].Should().Be(85.0m);
        result.ValueMetrics["CustomerSatisfaction"].Should().Be(4.2m);
        
        result.BusinessHighlights.Should().Contain("Delivered authentication feature ahead of schedule");
        result.BusinessHighlights.Should().Contain("Reduced technical debt by 20%");
        result.BusinessHighlights.Should().Contain("Improved system performance by 35%");
        
        result.StrategicRecommendations.Should().Contain("Focus next sprint on high-value customer-facing features");
        result.StrategicRecommendations.Should().Contain("Consider A/B testing for new UI components");
    }
}

