using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Tests.Application.Agents.TestHelpers;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Tests.Application.Agents;

public class ProductAgentTests
{
    [Fact]
    public async System.Threading.Tasks.Task RunAsync_WithBacklogItems_ReturnsOutput()
    {
        // Arrange
        var mockLlmClient = new Mock<ILlmClient>();
        var mockVectorStore = new Mock<IVectorStore>();

        mockVectorStore.Setup(v => v.RetrieveContextAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Context data");

        var validJson = @"{
  ""items"": [
    {
      ""itemId"": 1,
      ""title"": ""Story 1"",
      ""priority"": 90,
      ""rationale"": ""High value""
    },
    {
      ""itemId"": 2,
      ""title"": ""Story 2"",
      ""priority"": 75,
      ""rationale"": ""Medium value""
    },
    {
      ""itemId"": 3,
      ""title"": ""Story 3"",
      ""priority"": 60,
      ""rationale"": ""Lower value""
    }
  ],
  ""confidence"": 0.85,
  ""summary"": ""Generated prioritization output""
}";
        mockLlmClient.Setup(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validJson);

        var parser = AgentParserTestHelper.CreateRealParser();
        var mockLogger = new Mock<ILogger<ProductAgent>>();
        var agent = new ProductAgent(mockLlmClient.Object, mockVectorStore.Object, parser, mockLogger.Object);
        var backlogItems = new List<string> { "Story 1", "Story 2", "Story 3" };
        var recentCompletions = new List<string> { "Completed Story X" };

        // Act
        var result = await agent.RunAsync(1, backlogItems, recentCompletions, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0);
        result.Rationale.Should().NotBeNullOrEmpty();

        mockVectorStore.Verify(v => v.RetrieveContextAsync(
            1, "product", 5, It.IsAny<CancellationToken>()), Times.Once);
        
        mockLlmClient.Verify(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

