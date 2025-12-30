using MediatR;

namespace IntelliPM.Application.Projects.Commands;

public record CreateProjectCommand(
    string Name,
    string Description,
    string Type,
    int SprintDurationDays,
    int OwnerId,
    string Status = "Active",
    DateTimeOffset? StartDate = null,
    List<int>? MemberIds = null
) : IRequest<CreateProjectResponse>;

public record CreateProjectResponse(int Id, string Name, string Description, string Type);

