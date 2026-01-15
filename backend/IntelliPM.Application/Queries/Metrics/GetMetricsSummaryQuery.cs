using MediatR;

namespace IntelliPM.Application.Queries.Metrics;

public class GetMetricsSummaryQuery : IRequest<MetricsSummaryDto>
{
    public int? ProjectId { get; set; } // Optional: filter by project
}

public class MetricsSummaryDto
{
    // Project metrics
    public int TotalProjects { get; set; }
    
    // Task metrics
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public int TodoTasks { get; set; }
    public int OpenTasks { get; set; } // TodoTasks + InProgressTasks + BlockedTasks
    
    public double CompletionPercentage { get; set; }
    public double AverageCompletionTimeHours { get; set; }
    
    // Sprint metrics
    public int TotalSprints { get; set; }
    public int ActiveSprints { get; set; }
    public double Velocity { get; set; } // Average story points per sprint
    
    // Defect metrics
    public int DefectsCount { get; set; } // Open defects count
    public int TotalDefects { get; set; }
    
    // Agent metrics
    public int TotalAgentExecutions { get; set; }
    public double AgentSuccessRate { get; set; }
    public int AverageAgentResponseTimeMs { get; set; }
    
    // Trend data (comparing current period to previous period)
    public TrendData? Trends { get; set; }
}

public class TrendData
{
    public double ProjectsTrend { get; set; }
    public double SprintsTrend { get; set; }
    public double OpenTasksTrend { get; set; }
    public double BlockedTasksTrend { get; set; }
    public double DefectsTrend { get; set; }
    public double VelocityTrend { get; set; }
}

