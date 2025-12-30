using MediatR;

namespace IntelliPM.Application.Queries.Metrics;

public class GetMetricsSummaryQuery : IRequest<MetricsSummaryDto>
{
    public int? ProjectId { get; set; } // Optional: filter by project
}

public class MetricsSummaryDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public int TodoTasks { get; set; }
    
    public double CompletionPercentage { get; set; }
    public double AverageCompletionTimeHours { get; set; }
    
    public int TotalSprints { get; set; }
    public int ActiveSprints { get; set; }
    
    public int TotalAgentExecutions { get; set; }
    public double AgentSuccessRate { get; set; }
    public int AverageAgentResponseTimeMs { get; set; }
}

