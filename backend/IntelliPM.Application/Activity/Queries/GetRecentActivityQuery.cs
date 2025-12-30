using MediatR;

namespace IntelliPM.Application.Activity.Queries;

public class GetRecentActivityQuery : IRequest<GetRecentActivityResponse>
{
    public int? Limit { get; set; } = 10;
    public int? ProjectId { get; set; } // Optional: filter by project
    public int? UserId { get; set; } // Optional: filter by user's projects
}

public class GetRecentActivityResponse
{
    public List<ActivityDto> Activities { get; set; } = new();
}

public class ActivityDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? EntityName { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
