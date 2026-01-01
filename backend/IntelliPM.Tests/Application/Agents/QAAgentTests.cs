using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Tests.Application.Agents.TestHelpers;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Tests.Application.Agents;

public class QAAgentTests
{
    [Fact]
    public async Task RunAsync_ShouldReturnValidOutput_WhenGivenValidProjectId()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("QA context: Historical defect patterns and quality metrics.");

        var validJson = @"{
  ""defectAnalysis"": ""Overall quality is good. Found recurring authentication issues. Recommend enhanced unit test coverage."",
  ""patterns"": [
    {
      ""pattern"": ""Authentication Errors"",
      ""frequency"": 3,
      ""severity"": ""High"",
      ""suggestion"": ""Review auth token handling""
    }
  ],
  ""recommendations"": [""Rec 1"", ""Rec 2"", ""Rec 3""],
  ""confidence"": 0.84
}";
        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validJson);

        var parser = AgentParserTestHelper.CreateRealParser();
        var mockLogger = new Mock<ILogger<QAAgent>>();
        var agent = new QAAgent(mockLlmClient.Object, mockVectorStore.Object, parser, mockLogger.Object);
        var projectId = 1;
        var recentDefects = new List<string>
        {
            "Critical: Authentication token expired prematurely",
            "High: UI layout breaks on mobile devices",
            "Medium: Slow API response time"
        };
        var defectStats = new Dictionary<string, int>
        {
            { "Open", 8 },
            { "Critical", 2 },
            { "High", 3 },
            { "Medium", 3 }
        };

        // Act
        var result = await agent.RunAsync(projectId, recentDefects, defectStats, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DefectAnalysis.Should().NotBeNullOrEmpty();
        result.Patterns.Should().NotBeNull();
        result.Patterns.Should().HaveCountGreaterThan(0);
        result.Recommendations.Should().NotBeNull();
        result.Recommendations.Should().HaveCountGreaterThan(0);
        result.Confidence.Should().BeGreaterThan(0);
        result.Confidence.Should().BeLessOrEqualTo(1);

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            projectId, "qa", 5, It.IsAny<CancellationToken>()), Times.Once);
        
        mockLlmClient.Verify(l => l.GenerateTextAsync(
            It.Is<string>(p => p.Contains("RECENT DEFECTS") && p.Contains("DEFECT STATISTICS")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldIncludeRAGContext_WhenExecuted()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();
        var expectedContext = "Historical QA data: Previous analysis showed improvement in test coverage.";

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContext);

        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("QA analysis complete.");

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<QAAgent>>();
        var agent = new QAAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act
        await agent.RunAsync(1, new List<string>(), new Dictionary<string, int>(), CancellationToken.None);

        // Assert
        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            1, "qa", 5, It.IsAny<CancellationToken>()), Times.Once);
        
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
            .ThrowsAsync(new InvalidOperationException("VectorStore database error"));

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<QAAgent>>();
        var agent = new QAAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await agent.RunAsync(1, new List<string>(), new Dictionary<string, int>(), CancellationToken.None));

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
            .ThrowsAsync(new TaskCanceledException("LLM timeout after 30s"));

        var mockParser = new Mock<IAgentOutputParser>();
        var mockLogger = new Mock<ILogger<QAAgent>>();
        var agent = new QAAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await agent.RunAsync(1, new List<string>(), new Dictionary<string, int>(), CancellationToken.None));

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldSerializeDefectStats_WhenBuildingPrompt()
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
        var mockLogger = new Mock<ILogger<QAAgent>>();
        var agent = new QAAgent(mockLlmClient.Object, mockVectorStore.Object, mockParser.Object, mockLogger.Object);
        var recentDefects = new List<string> { "Critical: System crash", "High: Data loss bug" };
        var defectStats = new Dictionary<string, int>
        {
            { "Open", 15 },
            { "Critical", 3 }
        };

        // Act
        await agent.RunAsync(1, recentDefects, defectStats, CancellationToken.None);

        // Assert
        capturedPrompt.Should().NotBeNull();
        capturedPrompt.Should().Contain("RECENT DEFECTS");
        capturedPrompt.Should().Contain("System crash");
        capturedPrompt.Should().Contain("DEFECT STATISTICS");
        capturedPrompt.Should().Contain("Open");
        capturedPrompt.Should().Contain("15");
        capturedPrompt.Should().Contain("Critical");
        capturedPrompt.Should().Contain("3");
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
  ""defectAnalysis"": ""Defect analysis output"",
  ""patterns"": [
    {
      ""pattern"": ""Authentication Errors"",
      ""frequency"": 3,
      ""severity"": ""High"",
      ""suggestion"": ""Review auth logic""
    },
    {
      ""pattern"": ""UI Layout Issues"",
      ""frequency"": 2,
      ""severity"": ""Medium"",
      ""suggestion"": ""Review responsive design""
    }
  ],
  ""recommendations"": [""Rec 1"", ""Rec 2"", ""Rec 3""],
  ""confidence"": 0.84
}";
        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validJson);

        var parser = AgentParserTestHelper.CreateRealParser();
        var mockLogger = new Mock<ILogger<QAAgent>>();
        var agent = new QAAgent(mockLlmClient.Object, mockVectorStore.Object, parser, mockLogger.Object);

        // Act
        var result = await agent.RunAsync(1, new List<string>(), new Dictionary<string, int>(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DefectAnalysis.Should().Be("Defect analysis output");
        result.Patterns.Should().NotBeNull();
        result.Patterns.Should().HaveCount(2);
        result.Patterns.Should().Contain(p => p.Pattern == "Authentication Errors");
        result.Patterns.Should().Contain(p => p.Pattern == "UI Layout Issues");
        result.Recommendations.Should().NotBeNull();
        result.Recommendations.Should().HaveCount(3);
        result.Confidence.Should().Be(0.84m);
    }

    [Fact]
    public async Task RunAsync_ShouldIncludeDefectPatterns_WithCorrectStructure()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Context");

        var validJson = @"{
  ""defectAnalysis"": ""Analysis"",
  ""patterns"": [
    {
      ""pattern"": ""Authentication Errors"",
      ""frequency"": 3,
      ""severity"": ""High"",
      ""suggestion"": ""Review auth token handling and expiration logic""
    },
    {
      ""pattern"": ""UI Layout Issues"",
      ""frequency"": 5,
      ""severity"": ""Medium"",
      ""suggestion"": ""Review responsive design patterns""
    }
  ],
  ""recommendations"": [""Rec 1"", ""Rec 2"", ""Rec 3""],
  ""confidence"": 0.84
}";
        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validJson);

        var parser = AgentParserTestHelper.CreateRealParser();
        var mockLogger = new Mock<ILogger<QAAgent>>();
        var agent = new QAAgent(mockLlmClient.Object, mockVectorStore.Object, parser, mockLogger.Object);

        // Act
        var result = await agent.RunAsync(1, new List<string>(), new Dictionary<string, int>(), CancellationToken.None);

        // Assert
        result.Patterns.Should().HaveCount(2);
        var authPattern = result.Patterns.First(p => p.Pattern == "Authentication Errors");
        authPattern.Frequency.Should().Be(3);
        authPattern.Severity.Should().Be("High");
        authPattern.Suggestion.Should().Contain("auth token");
        
        var uiPattern = result.Patterns.First(p => p.Pattern == "UI Layout Issues");
        uiPattern.Frequency.Should().Be(5);
        uiPattern.Severity.Should().Be("Medium");
        uiPattern.Suggestion.Should().Contain("responsive design");
    }
}

