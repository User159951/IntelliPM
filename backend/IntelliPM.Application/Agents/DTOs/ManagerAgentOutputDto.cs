namespace IntelliPM.Application.Agents.DTOs;

/// <summary>
/// DTO for Manager Agent output parsed from LLM JSON response.
/// </summary>
public class ManagerAgentOutputDto
{
    /// <summary>
    /// Executive summary text.
    /// </summary>
    public string ExecutiveSummary { get; set; } = string.Empty;

    /// <summary>
    /// List of key decisions that need to be made.
    /// </summary>
    public List<string> KeyDecisions { get; set; } = new();

    /// <summary>
    /// List of highlights or achievements.
    /// </summary>
    public List<string> Highlights { get; set; } = new();

    /// <summary>
    /// Confidence score between 0.0 and 1.0.
    /// </summary>
    public decimal Confidence { get; set; }
}

