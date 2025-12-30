using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace IntelliPM.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin for analyzing and formatting task descriptions
/// </summary>
public class TaskQualityPlugin
{
    /// <summary>
    /// Analyzes a task description and checks for missing information
    /// </summary>
    [KernelFunction]
    [Description("Analyzes a task description and checks for missing information like acceptance criteria, definition of done, effort estimate, and owner")]
    public async System.Threading.Tasks.Task<TaskQualityAnalysis> AnalyzeTaskQuality(
        [Description("The task description to analyze")] string taskDescription)
    {
        await System.Threading.Tasks.Task.CompletedTask; // Make async for SK compatibility
        
        var missingFields = new List<string>();
        var suggestions = new List<string>();
        
        // Check for required components
        if (!taskDescription.Contains("acceptance criteria", StringComparison.OrdinalIgnoreCase))
        {
            missingFields.Add("Acceptance Criteria");
            suggestions.Add("Add clear acceptance criteria that define when this task is complete");
        }
        
        if (!taskDescription.Contains("definition of done", StringComparison.OrdinalIgnoreCase) 
            && !taskDescription.Contains("DoD", StringComparison.OrdinalIgnoreCase))
        {
            missingFields.Add("Definition of Done");
            suggestions.Add("Include a Definition of Done checklist");
        }
        
        if (!taskDescription.Contains("story point", StringComparison.OrdinalIgnoreCase) 
            && !taskDescription.Contains("effort", StringComparison.OrdinalIgnoreCase)
            && !taskDescription.Contains("estimate", StringComparison.OrdinalIgnoreCase))
        {
            missingFields.Add("Effort Estimate");
            suggestions.Add("Add story points or effort estimate (e.g., 1, 2, 3, 5, 8, 13)");
        }
        
        if (!taskDescription.Contains("owner", StringComparison.OrdinalIgnoreCase) 
            && !taskDescription.Contains("assignee", StringComparison.OrdinalIgnoreCase)
            && !taskDescription.Contains("assigned", StringComparison.OrdinalIgnoreCase))
        {
            missingFields.Add("Owner/Assignee");
            suggestions.Add("Specify who will work on this task");
        }

        // Check for technical details
        if (!taskDescription.Contains("technical", StringComparison.OrdinalIgnoreCase)
            && !taskDescription.Contains("implementation", StringComparison.OrdinalIgnoreCase)
            && taskDescription.Length < 50)
        {
            suggestions.Add("Add more technical details about implementation approach");
        }

        // Check for clarity
        if (taskDescription.Length < 20)
        {
            suggestions.Add("Task description is too brief - add more context and details");
        }
        
        // Calculate quality score (0-10)
        var score = 10 - (missingFields.Count * 2);
        var isComplete = missingFields.Count == 0;
        
        var feedback = score >= 8 
            ? "✅ Great task description! All key information present."
            : score >= 5
                ? $"⚠️ Task needs improvement. Missing: {string.Join(", ", missingFields)}"
                : $"❌ Task description is incomplete. Critical missing: {string.Join(", ", missingFields)}";
        
        return new TaskQualityAnalysis
        {
            QualityScore = Math.Max(score, 0),
            MissingFields = missingFields,
            Suggestions = suggestions,
            Feedback = feedback,
            IsComplete = isComplete
        };
    }
    
    /// <summary>
    /// Formats a raw task description into a well-structured task
    /// </summary>
    [KernelFunction]
    [Description("Formats a raw task description into a well-structured task with title, description, priority, and acceptance criteria")]
    public async System.Threading.Tasks.Task<FormattedTask> FormatTask(
        [Description("Original task description")] string description,
        [Description("Clean task title")] string title,
        [Description("Priority: High, Medium, or Low")] string priority)
    {
        await System.Threading.Tasks.Task.CompletedTask; // Make async for SK compatibility
        
        // Validate and normalize priority
        var normalizedPriority = priority.Trim().ToLower() switch
        {
            "high" or "urgent" or "critical" => "High",
            "medium" or "normal" or "moderate" => "Medium",
            "low" or "minor" => "Low",
            _ => "Medium" // Default
        };
        
        // Generate smart acceptance criteria based on task type
        var acceptanceCriteria = GenerateAcceptanceCriteria(description, title);
        
        return new FormattedTask
        {
            Title = title.Trim(),
            Description = description.Trim(),
            Priority = normalizedPriority,
            Status = "Todo",
            CreatedAt = DateTimeOffset.UtcNow,
            AcceptanceCriteria = acceptanceCriteria,
            EstimatedStoryPoints = EstimateStoryPoints(description, title)
        };
    }

    /// <summary>
    /// Generates smart acceptance criteria based on task content
    /// </summary>
    private List<string> GenerateAcceptanceCriteria(string description, string title)
    {
        var criteria = new List<string>();
        var lowerDescription = description.ToLower();
        var lowerTitle = title.ToLower();

        // Feature-specific criteria
        if (lowerTitle.Contains("api") || lowerDescription.Contains("api"))
        {
            criteria.Add("API endpoint is created and returns correct response format");
            criteria.Add("API is documented with Swagger/OpenAPI");
            criteria.Add("Error handling is implemented for edge cases");
        }
        else if (lowerTitle.Contains("ui") || lowerTitle.Contains("frontend") || lowerDescription.Contains("ui"))
        {
            criteria.Add("UI components are created and match design specifications");
            criteria.Add("UI is responsive on mobile and desktop");
            criteria.Add("User interactions work as expected");
        }
        else if (lowerTitle.Contains("test") || lowerDescription.Contains("test"))
        {
            criteria.Add("Test cases cover all critical paths");
            criteria.Add("Tests pass with 100% success rate");
            criteria.Add("Code coverage meets project standards");
        }
        else if (lowerTitle.Contains("bug") || lowerTitle.Contains("fix"))
        {
            criteria.Add("Bug is reproducible and root cause identified");
            criteria.Add("Fix is implemented and tested");
            criteria.Add("Regression tests added to prevent recurrence");
        }
        else
        {
            // Generic criteria
            criteria.Add("Implementation meets requirements specified in description");
            criteria.Add("Code is reviewed and approved by at least one team member");
            criteria.Add("Changes are tested and working as expected");
        }

        // Always add documentation criterion
        criteria.Add("Documentation is updated if needed");

        return criteria;
    }

    /// <summary>
    /// Estimates story points based on task complexity indicators
    /// </summary>
    private int? EstimateStoryPoints(string description, string title)
    {
        var text = $"{title} {description}".ToLower();
        var complexityScore = 0;

        // Complexity indicators
        if (text.Contains("database") || text.Contains("migration")) complexityScore += 2;
        if (text.Contains("api")) complexityScore += 1;
        if (text.Contains("authentication") || text.Contains("security")) complexityScore += 3;
        if (text.Contains("integration") || text.Contains("third-party")) complexityScore += 2;
        if (text.Contains("refactor")) complexityScore += 2;
        if (text.Contains("complex") || text.Contains("difficult")) complexityScore += 2;
        if (text.Contains("multiple") || text.Contains("several")) complexityScore += 1;
        
        // Length-based complexity
        if (description.Length > 200) complexityScore += 1;
        if (description.Length > 500) complexityScore += 2;

        // Map to Fibonacci story points
        return complexityScore switch
        {
            0 => 1,
            1 or 2 => 2,
            3 or 4 => 3,
            5 or 6 => 5,
            7 or 8 => 8,
            _ => 13
        };
    }
}

/// <summary>
/// Result of task quality analysis
/// </summary>
public class TaskQualityAnalysis
{
    /// <summary>
    /// Quality score from 0-10
    /// </summary>
    public int QualityScore { get; set; }
    
    /// <summary>
    /// List of fields missing from the task description
    /// </summary>
    public List<string> MissingFields { get; set; } = new();
    
    /// <summary>
    /// Suggestions for improving the task
    /// </summary>
    public List<string> Suggestions { get; set; } = new();
    
    /// <summary>
    /// Human-readable feedback on task quality
    /// </summary>
    public string Feedback { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the task description is complete
    /// </summary>
    public bool IsComplete { get; set; }
}

/// <summary>
/// A formatted and structured task
/// </summary>
public class FormattedTask
{
    /// <summary>
    /// Clean, concise task title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed task description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Priority level: High, Medium, or Low
    /// </summary>
    public string Priority { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the task
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// When the task was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// List of acceptance criteria for the task
    /// </summary>
    public List<string> AcceptanceCriteria { get; set; } = new();
    
    /// <summary>
    /// Estimated story points (1, 2, 3, 5, 8, 13)
    /// </summary>
    public int? EstimatedStoryPoints { get; set; }
}

