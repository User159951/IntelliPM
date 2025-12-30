using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Handler for DeleteMilestoneCommand.
/// Deletes a milestone from the database.
/// </summary>
public class DeleteMilestoneCommandHandler : IRequestHandler<DeleteMilestoneCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteMilestoneCommandHandler> _logger;

    public DeleteMilestoneCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteMilestoneCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<Unit> Handle(DeleteMilestoneCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Get milestone by ID
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.OrganizationId == organizationId, cancellationToken);

        if (milestone == null)
        {
            throw new NotFoundException($"Milestone with ID {request.Id} not found");
        }

        // Hard delete
        _unitOfWork.Repository<Milestone>().Delete(milestone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Milestone {MilestoneId} deleted by user {UserId}",
            milestone.Id,
            _currentUserService.GetUserId());

        return Unit.Value;
    }
}

