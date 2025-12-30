using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for RemoveSprintFromReleaseCommand.
/// Removes a sprint from its release.
/// </summary>
public class RemoveSprintFromReleaseCommandHandler : IRequestHandler<RemoveSprintFromReleaseCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveSprintFromReleaseCommandHandler> _logger;

    public RemoveSprintFromReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<RemoveSprintFromReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<Unit> Handle(RemoveSprintFromReleaseCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Get sprint and verify ownership
        var sprint = await _unitOfWork.Repository<Sprint>().GetByIdAsync(request.SprintId, cancellationToken);
        if (sprint == null || sprint.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Sprint with ID {request.SprintId} not found");
        }

        // Check if sprint is actually in a release
        if (!sprint.ReleaseId.HasValue)
        {
            throw new ValidationException("Sprint is not assigned to any release");
        }

        var releaseId = sprint.ReleaseId.Value;

        // Remove sprint from release
        sprint.ReleaseId = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Sprint {SprintId} removed from Release {ReleaseId} by user {UserId}",
            request.SprintId,
            releaseId,
            _currentUserService.GetUserId());

        return Unit.Value;
    }
}

