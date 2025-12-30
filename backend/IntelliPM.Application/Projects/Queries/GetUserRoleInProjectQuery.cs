using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Projects.Queries;

public record GetUserRoleInProjectQuery(int ProjectId, int UserId) : IRequest<ProjectRole?>;

