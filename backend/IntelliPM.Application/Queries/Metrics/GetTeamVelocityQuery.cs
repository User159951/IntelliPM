using MediatR;

namespace IntelliPM.Application.Queries.Metrics;

public class GetTeamVelocityQuery : IRequest<TeamVelocityResponse>
{
    public int? ProjectId { get; set; }
}

public class TeamVelocityResponse
{
    public List<TeamVelocityData> Velocity { get; set; } = new();
}

public class TeamVelocityData
{
    public DateTimeOffset Date { get; set; }
    public int StoryPoints { get; set; }
    public int SprintNumber { get; set; }
}
