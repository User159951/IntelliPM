using MediatR;

namespace IntelliPM.Application.Projects.Queries;

public record GetProjectMembersQuery(int ProjectId, int CurrentUserId) : IRequest<List<ProjectMemberDto>>;

