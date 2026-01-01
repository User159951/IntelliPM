namespace IntelliPM.Application.Agents.DTOs;

/// <summary>
/// DTO for Product Agent output parsed from LLM JSON response.
/// </summary>
public class ProductAgentOutputDto
{
    /// <summary>
    /// List of prioritized items with rankings and rationale.
    /// </summary>
    public List<PrioritizedItemDto> Items { get; set; } = new();

    /// <summary>
    /// Confidence score between 0.0 and 1.0.
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Overall summary/rationale for the prioritization.
    /// </summary>
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// DTO for a single prioritized item.
/// </summary>
public class PrioritizedItemDto
{
    /// <summary>
    /// ID of the backlog item.
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// Title of the item.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Priority ranking (higher = more important, typically 1-100).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Rationale explaining why this item has this priority.
    /// </summary>
    public string Rationale { get; set; } = string.Empty;
}

