namespace IntelliPM.Application.Agents.DTOs;

/// <summary>
/// DTO for Business Agent output parsed from LLM JSON response.
/// </summary>
public class BusinessAgentOutputDto
{
    /// <summary>
    /// Business value delivery summary text.
    /// </summary>
    public string ValueDeliverySummary { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary of value metrics (e.g., ROI, TimeToMarket, CustomerSatisfaction).
    /// </summary>
    public Dictionary<string, decimal> ValueMetrics { get; set; } = new();

    /// <summary>
    /// List of business highlights.
    /// </summary>
    public List<string> BusinessHighlights { get; set; } = new();

    /// <summary>
    /// List of strategic recommendations.
    /// </summary>
    public List<string> StrategicRecommendations { get; set; } = new();

    /// <summary>
    /// Confidence score between 0.0 and 1.0.
    /// </summary>
    public decimal Confidence { get; set; }
}

