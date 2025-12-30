using MediatR;

namespace IntelliPM.Application.Queries.Metrics;

public class GetSprintBurndownQuery : IRequest<SprintBurndownResponse>
{
    public int SprintId { get; set; }
}

public class SprintBurndownResponse
{
    public List<BurndownDayData> Days { get; set; } = new();
}

public class BurndownDayData
{
    public int Day { get; set; }
    public int Ideal { get; set; }
    public int Actual { get; set; }
}
