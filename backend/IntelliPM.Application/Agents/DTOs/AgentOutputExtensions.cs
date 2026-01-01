using IntelliPM.Application.Agents.Services;

namespace IntelliPM.Application.Agents.DTOs;

/// <summary>
/// Extension methods for converting DTOs to agent output records.
/// </summary>
public static class AgentOutputExtensions
{
    /// <summary>
    /// Converts ProductAgentOutputDto to ProductAgentOutput record.
    /// </summary>
    public static ProductAgentOutput ToProductAgentOutput(this ProductAgentOutputDto dto)
    {
        return new ProductAgentOutput(
            PrioritizedItems: dto.Items.Select(i => new PrioritizedItem(
                ItemId: i.ItemId,
                Title: i.Title,
                Priority: i.Priority,
                Rationale: i.Rationale
            )).ToList(),
            Confidence: dto.Confidence,
            Rationale: dto.Summary
        );
    }

    /// <summary>
    /// Converts DeliveryAgentOutputDto to DeliveryAgentOutput record.
    /// </summary>
    public static DeliveryAgentOutput ToDeliveryAgentOutput(this DeliveryAgentOutputDto dto)
    {
        return new DeliveryAgentOutput(
            RiskAssessment: dto.RiskAssessment,
            RecommendedActions: dto.RecommendedActions,
            Confidence: dto.Confidence
        );
    }

    /// <summary>
    /// Converts ManagerAgentOutputDto to ManagerAgentOutput record.
    /// </summary>
    public static ManagerAgentOutput ToManagerAgentOutput(this ManagerAgentOutputDto dto)
    {
        return new ManagerAgentOutput(
            ExecutiveSummary: dto.ExecutiveSummary,
            KeyDecisionsNeeded: dto.KeyDecisions,
            Highlights: dto.Highlights,
            Confidence: dto.Confidence
        );
    }

    /// <summary>
    /// Converts QAAgentOutputDto to QAAgentOutput record.
    /// </summary>
    public static QAAgentOutput ToQAAgentOutput(this QAAgentOutputDto dto)
    {
        return new QAAgentOutput(
            DefectAnalysis: dto.DefectAnalysis,
            Patterns: dto.Patterns.Select(p => new DefectPattern(
                Pattern: p.Pattern,
                Frequency: p.Frequency,
                Severity: p.Severity,
                Suggestion: p.Suggestion
            )).ToList(),
            Recommendations: dto.Recommendations,
            Confidence: dto.Confidence
        );
    }

    /// <summary>
    /// Converts BusinessAgentOutputDto to BusinessAgentOutput record.
    /// </summary>
    public static BusinessAgentOutput ToBusinessAgentOutput(this BusinessAgentOutputDto dto)
    {
        return new BusinessAgentOutput(
            ValueDeliverySummary: dto.ValueDeliverySummary,
            ValueMetrics: dto.ValueMetrics,
            BusinessHighlights: dto.BusinessHighlights,
            StrategicRecommendations: dto.StrategicRecommendations,
            Confidence: dto.Confidence
        );
    }
}

