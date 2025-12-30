using MediatR;

namespace IntelliPM.Application.Projects.Queries;

public record GetProjectByIdQuery(int ProjectId) : IRequest<GetProjectByIdResponse>;

public record GetProjectByIdResponse(
    int Id,
    string Name,
    string Description,
    string Type,
    string Status,
    List<ProjectMemberDto> Members,
    DateTimeOffset CreatedAt
);

