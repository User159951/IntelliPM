using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for AddSprintToReleaseCommand.
/// Adds a sprint to a release with validation.
/// </summary>
public class AddSprintToReleaseCommandHandler : IRequestHandler<AddSprintToReleaseCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AddSprintToReleaseCommandHandler> _logger;

    public AddSprintToReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<AddSprintToReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<Unit> Handle(AddSprintToReleaseCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Get release and verify ownership
        var release = await _unitOfWork.Repository<Release>().GetByIdAsync(request.ReleaseId, cancellationToken);
        if (release == null || release.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Release with ID {request.ReleaseId} not found");
        }

        // Get sprint and verify ownership
        var sprint = await _unitOfWork.Repository<Sprint>().GetByIdAsync(request.SprintId, cancellationToken);
        if (sprint == null || sprint.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Sprint with ID {request.SprintId} not found");
        }

        // Verify sprint belongs to same project
        if (sprint.ProjectId != release.ProjectId)
        {
            throw new ValidationException("Sprint must belong to the same project as the release");
        }

        // Validate using domain method
        if (!release.CanAddSprint(sprint))
        {
            throw new ValidationException("Sprint cannot be added to this release. Release may be deployed or cancelled, or sprint may belong to a different project.");
        }

        // Check if sprint is already in another release
        if (sprint.ReleaseId.HasValue && sprint.ReleaseId.Value != release.Id)
        {
            throw new ValidationException($"Sprint is already assigned to another release (ID: {sprint.ReleaseId.Value})");
        }

        // Link sprint to release
        sprint.ReleaseId = release.Id;
        // Note: Sprint entity doesn't have UpdatedAt, but we can track changes via RowVersion

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Sprint {SprintId} added to Release {ReleaseId} by user {UserId}",
            request.SprintId,
            request.ReleaseId,
            _currentUserService.GetUserId());

        return Unit.Value;
    }
}

