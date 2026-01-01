namespace IntelliPM.Application.Agents.DTOs;

/// <summary>
/// DTO for Delivery Agent output parsed from LLM JSON response.
/// </summary>
public class DeliveryAgentOutputDto
{
    /// <summary>
    /// Risk assessment text describing the delivery status and risks.
    /// </summary>
    public string RiskAssessment { get; set; } = string.Empty;

    /// <summary>
    /// List of recommended actions to mitigate risks or improve delivery.
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();

    /// <summary>
    /// List of key highlights or positive indicators.
    /// </summary>
    public List<string> Highlights { get; set; } = new();

    /// <summary>
    /// Confidence score between 0.0 and 1.0.
    /// </summary>
    public decimal Confidence { get; set; }
}

