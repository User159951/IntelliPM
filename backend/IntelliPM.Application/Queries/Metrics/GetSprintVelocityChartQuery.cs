using MediatR;

namespace IntelliPM.Application.Queries.Metrics;

public class GetSprintVelocityChartQuery : IRequest<SprintVelocityChartResponse>
{
    public int? ProjectId { get; set; }
}

public class SprintVelocityChartResponse
{
    public List<SprintVelocityData> Sprints { get; set; } = new();
}

public class SprintVelocityData
{
    public int Number { get; set; }
    public int StoryPoints { get; set; }
    public DateTimeOffset CompletedDate { get; set; }
}
