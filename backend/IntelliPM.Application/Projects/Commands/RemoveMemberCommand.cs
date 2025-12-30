using MediatR;

namespace IntelliPM.Application.Projects.Commands;

public record RemoveMemberCommand(int ProjectId, int CurrentUserId, int UserId) : IRequest<Unit>;

