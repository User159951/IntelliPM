using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Admin.DeadLetterQueue.Commands;

public class DeleteDeadLetterMessageCommandHandler : IRequestHandler<DeleteDeadLetterMessageCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteDeadLetterMessageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<Unit> Handle(DeleteDeadLetterMessageCommand request, CancellationToken cancellationToken)
    {
        // Ensure only admins can delete dead letter messages
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can delete dead letter queue messages.");
        }

        var dlqRepo = _unitOfWork.Repository<DeadLetterMessage>();

        // Find the dead letter message
        var deadLetterMessage = await dlqRepo.Query()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (deadLetterMessage == null)
        {
            throw new NotFoundException($"Dead letter message with ID {request.Id} not found.");
        }

        // Remove from DLQ
        dlqRepo.Delete(deadLetterMessage);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

