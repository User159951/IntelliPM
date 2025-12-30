using MediatR;

namespace IntelliPM.Application.Admin.DeadLetterQueue.Commands;

public record RetryDeadLetterMessageCommand(Guid Id) : IRequest<Unit>;

