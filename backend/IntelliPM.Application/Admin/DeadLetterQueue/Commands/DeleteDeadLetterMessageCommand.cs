using MediatR;

namespace IntelliPM.Application.Admin.DeadLetterQueue.Commands;

public record DeleteDeadLetterMessageCommand(Guid Id) : IRequest<Unit>;

