using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using IntelliPM.Application.Agents.DTOs;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Agents.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IntelliPM.Tests.Application.Agents;

public class AgentOutputParserTests
{
    private readonly IAgentOutputParser _parser;
    private readonly IServiceProvider _serviceProvider;

    public AgentOutputParserTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddValidatorsFromAssemblyContaining<ProductAgentOutputValidator>();
        services.AddScoped<IAgentOutputParser, AgentOutputParser>();
        _serviceProvider = services.BuildServiceProvider();
        _parser = _serviceProvider.GetRequiredService<IAgentOutputParser>();
    }

    #region ProductAgentOutputDto Parsing

    [Fact]
    public void TryParse_ProductAgentOutput_WithValidJson_ReturnsTrue()
    {
        // Arrange
        var json = @"{
            ""items"": [
                {
                    ""itemId"": 1,
                    ""title"": ""Feature A"",
                    ""priority"": 90,
                    ""rationale"": ""High ROI""
                },
                {
                    ""itemId"": 2,
                    ""title"": ""Feature B"",
                    ""priority"": 75,
                    ""rationale"": ""Medium priority""
                }
            ],
            ""confidence"": 0.85,
            ""summary"": ""Top 2 prioritized items""
        }";

        // Act
        var success = _parser.TryParse<ProductAgentOutputDto>(json, out var result, out var errors);

        // Assert
        success.Should().BeTrue();
        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items[0].ItemId.Should().Be(1);
        result.Items[0].Title.Should().Be("Feature A");
        result.Items[0].Priority.Should().Be(90);
        result.Confidence.Should().Be(0.85m);
        result.Summary.Should().Be("Top 2 prioritized items");
    }

    [Fact]
    public void TryParse_ProductAgentOutput_WithInvalidJson_ReturnsFalse()
    {
        // Arrange
        var invalidJson = @"This is not valid JSON { invalid syntax }";

        // Act
        var success = _parser.TryParse<ProductAgentOutputDto>(invalidJson, out var result, out var errors);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("JSON parsing error"));
    }

    [Fact]
    public void TryParse_ProductAgentOutput_WithInvalidData_ValidationFails()
    {
        // Arrange - confidence out of range
        var json = @"{
            ""items"": [
                {
                    ""itemId"": 1,
                    ""title"": ""Feature A"",
                    ""priority"": 90,
                    ""rationale"": ""High ROI""
                }
            ],
            ""confidence"": 1.5,
            ""summary"": ""Summary""
        }";

        // Act
        var success = _parser.TryParse<ProductAgentOutputDto>(json, out var result, out var errors);

        // Assert
        success.Should().BeFalse();
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("Confidence") && e.Contains("between 0.0 and 1.0"));
    }

    [Fact]
    public void TryParse_ProductAgentOutput_WithMarkdownCodeBlock_ExtractsJson()
    {
        // Arrange
        var jsonWithMarkdown = @"Here is the JSON response:

```json
{
    ""items"": [
        {
            ""itemId"": 1,
            ""title"": ""Feature A"",
            ""priority"": 90,
            ""rationale"": ""High ROI""
        }
    ],
    ""confidence"": 0.85,
    ""summary"": ""Summary""
}
```

That's the response.";

        // Act
        var success = _parser.TryParse<ProductAgentOutputDto>(jsonWithMarkdown, out var result, out var errors);

        // Assert
        success.Should().BeTrue();
        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Confidence.Should().Be(0.85m);
    }

    #endregion

    #region DeliveryAgentOutputDto Parsing

    [Fact]
    public void TryParse_DeliveryAgentOutput_WithValidJson_ReturnsTrue()
    {
        // Arrange
        var json = @"{
            ""riskAssessment"": ""Project is on track"",
            ""recommendedActions"": [
                ""Action 1"",
                ""Action 2"",
                ""Action 3""
            ],
            ""highlights"": [
                ""Highlight 1""
            ],
            ""confidence"": 0.82
        }";

        // Act
        var success = _parser.TryParse<DeliveryAgentOutputDto>(json, out var result, out var errors);

        // Assert
        success.Should().BeTrue();
        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.RiskAssessment.Should().Be("Project is on track");
        result.RecommendedActions.Should().HaveCount(3);
        result.Highlights.Should().HaveCount(1);
        result.Confidence.Should().Be(0.82m);
    }

    [Fact]
    public void TryParse_DeliveryAgentOutput_WithMissingRequiredFields_ValidationFails()
    {
        // Arrange - missing recommendedActions
        var json = @"{
            ""riskAssessment"": ""Assessment"",
            ""confidence"": 0.82
        }";

        // Act
        var success = _parser.TryParse<DeliveryAgentOutputDto>(json, out var result, out var errors);

        // Assert
        success.Should().BeFalse();
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("RecommendedActions") && e.Contains("required"));
    }

    #endregion

    #region ManagerAgentOutputDto Parsing

    [Fact]
    public void TryParse_ManagerAgentOutput_WithValidJson_ReturnsTrue()
    {
        // Arrange
        var json = @"{
            ""executiveSummary"": ""Executive summary text"",
            ""keyDecisions"": [
                ""Decision 1"",
                ""Decision 2""
            ],
            ""highlights"": [
                ""Highlight 1""
            ],
            ""confidence"": 0.88
        }";

        // Act
        var success = _parser.TryParse<ManagerAgentOutputDto>(json, out var result, out var errors);

        // Assert
        success.Should().BeTrue();
        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.ExecutiveSummary.Should().Be("Executive summary text");
        result.KeyDecisions.Should().HaveCount(2);
        result.Highlights.Should().HaveCount(1);
        result.Confidence.Should().Be(0.88m);
    }

    #endregion

    #region QAAgentOutputDto Parsing

    [Fact]
    public void TryParse_QAAgentOutput_WithValidJson_ReturnsTrue()
    {
        // Arrange
        var json = @"{
            ""defectAnalysis"": ""Quality is good"",
            ""patterns"": [
                {
                    ""pattern"": ""Auth Errors"",
                    ""frequency"": 3,
                    ""severity"": ""High"",
                    ""suggestion"": ""Review auth logic""
                }
            ],
            ""recommendations"": [
                ""Recommendation 1"",
                ""Recommendation 2""
            ],
            ""confidence"": 0.84
        }";

        // Act
        var success = _parser.TryParse<QAAgentOutputDto>(json, out var result, out var errors);

        // Assert
        success.Should().BeTrue();
        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.DefectAnalysis.Should().Be("Quality is good");
        result.Patterns.Should().HaveCount(1);
        result.Patterns[0].Pattern.Should().Be("Auth Errors");
        result.Patterns[0].Severity.Should().Be("High");
        result.Recommendations.Should().HaveCount(2);
        result.Confidence.Should().Be(0.84m);
    }

    [Fact]
    public void TryParse_QAAgentOutput_WithInvalidSeverity_ValidationFails()
    {
        // Arrange - invalid severity
        var json = @"{
            ""defectAnalysis"": ""Analysis"",
            ""patterns"": [
                {
                    ""pattern"": ""Auth Errors"",
                    ""frequency"": 3,
                    ""severity"": ""InvalidSeverity"",
                    ""suggestion"": ""Review auth logic""
                }
            ],
            ""recommendations"": [""Rec 1""],
            ""confidence"": 0.84
        }";

        // Act
        var success = _parser.TryParse<QAAgentOutputDto>(json, out var result, out var errors);

        // Assert
        success.Should().BeFalse();
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("Severity") && e.Contains("Critical, High, Medium, Low"));
    }

    #endregion

    #region BusinessAgentOutputDto Parsing

    [Fact]
    public void TryParse_BusinessAgentOutput_WithValidJson_ReturnsTrue()
    {
        // Arrange
        var json = @"{
            ""valueDeliverySummary"": ""Good value delivery"",
            ""valueMetrics"": {
                ""EstimatedROI"": 145.5,
                ""TimeToMarket"": 85.0,
                ""CustomerSatisfaction"": 4.2
            },
            ""businessHighlights"": [
                ""Highlight 1"",
                ""Highlight 2""
            ],
            ""strategicRecommendations"": [
                ""Recommendation 1"",
                ""Recommendation 2""
            ],
            ""confidence"": 0.87
        }";

        // Act
        var success = _parser.TryParse<BusinessAgentOutputDto>(json, out var result, out var errors);

        // Assert
        success.Should().BeTrue();
        errors.Should().BeEmpty();
        result.Should().NotBeNull();
        result!.ValueDeliverySummary.Should().Be("Good value delivery");
        result.ValueMetrics.Should().HaveCount(3);
        result.ValueMetrics["EstimatedROI"].Should().Be(145.5m);
        result.BusinessHighlights.Should().HaveCount(2);
        result.StrategicRecommendations.Should().HaveCount(2);
        result.Confidence.Should().Be(0.87m);
    }

    #endregion

    #region Error Handling

    [Fact]
    public void TryParse_WithNullInput_ReturnsFalse()
    {
        // Arrange
        string? nullJson = null;

        // Act
        var success = _parser.TryParse<ProductAgentOutputDto>(nullJson!, out var result, out var errors);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
        errors.Should().NotBeEmpty();
        errors.Should().Contain("null or empty");
    }

    [Fact]
    public void TryParse_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        var emptyJson = "";

        // Act
        var success = _parser.TryParse<ProductAgentOutputDto>(emptyJson, out var result, out var errors);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
        errors.Should().NotBeEmpty();
    }

    #endregion
}

