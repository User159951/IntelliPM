using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Projects.Commands;

public record InviteMemberCommand(int ProjectId, int CurrentUserId, string Email, ProjectRole Role) : IRequest<int>;

