namespace IntelliPM.Application.Agents.DTOs;

/// <summary>
/// DTO for QA Agent output parsed from LLM JSON response.
/// </summary>
public class QAAgentOutputDto
{
    /// <summary>
    /// Overall defect analysis and quality assessment text.
    /// </summary>
    public string DefectAnalysis { get; set; } = string.Empty;

    /// <summary>
    /// List of detected defect patterns.
    /// </summary>
    public List<DefectPatternDto> Patterns { get; set; } = new();

    /// <summary>
    /// List of recommendations for quality improvement.
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// Confidence score between 0.0 and 1.0.
    /// </summary>
    public decimal Confidence { get; set; }
}

/// <summary>
/// DTO for a defect pattern.
/// </summary>
public class DefectPatternDto
{
    /// <summary>
    /// Pattern description or name.
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Frequency of this pattern (number of occurrences).
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// Severity level (e.g., "Critical", "High", "Medium", "Low").
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Suggestion for addressing this pattern.
    /// </summary>
    public string Suggestion { get; set; } = string.Empty;
}

