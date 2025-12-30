using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Admin.DeadLetterQueue.Commands;

public class RetryDeadLetterMessageCommandHandler : IRequestHandler<RetryDeadLetterMessageCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RetryDeadLetterMessageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<Unit> Handle(RetryDeadLetterMessageCommand request, CancellationToken cancellationToken)
    {
        // Ensure only admins can retry dead letter messages
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can retry dead letter queue messages.");
        }

        var dlqRepo = _unitOfWork.Repository<DeadLetterMessage>();
        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();

        // Find the dead letter message
        var deadLetterMessage = await dlqRepo.Query()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (deadLetterMessage == null)
        {
            throw new NotFoundException($"Dead letter message with ID {request.Id} not found.");
        }

        // Create new outbox message from dead letter message
        var outboxMessage = OutboxMessage.Create(
            deadLetterMessage.EventType,
            deadLetterMessage.Payload,
            deadLetterMessage.IdempotencyKey);

        // Add to outbox
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);

        // Remove from DLQ
        dlqRepo.Delete(deadLetterMessage);

        // Save changes in a single transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

