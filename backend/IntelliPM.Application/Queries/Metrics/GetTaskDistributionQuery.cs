using MediatR;

namespace IntelliPM.Application.Queries.Metrics;

public class GetTaskDistributionQuery : IRequest<TaskDistributionResponse>
{
    public int? ProjectId { get; set; }
}

public class TaskDistributionResponse
{
    public List<TaskDistributionData> Distribution { get; set; } = new();
}

public class TaskDistributionData
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
