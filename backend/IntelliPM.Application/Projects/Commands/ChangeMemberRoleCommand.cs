using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Projects.Commands;

public record ChangeMemberRoleCommand(int ProjectId, int CurrentUserId, int UserId, ProjectRole NewRole) : IRequest<Unit>;

