using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for BulkAddSprintsToReleaseCommand.
/// Adds multiple sprints to a release in a single batch operation.
/// </summary>
public class BulkAddSprintsToReleaseCommandHandler : IRequestHandler<BulkAddSprintsToReleaseCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BulkAddSprintsToReleaseCommandHandler> _logger;

    public BulkAddSprintsToReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<BulkAddSprintsToReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<int> Handle(BulkAddSprintsToReleaseCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        if (request.SprintIds == null || request.SprintIds.Count == 0)
        {
            return 0;
        }

        // Get release and verify ownership
        var release = await _unitOfWork.Repository<Release>().GetByIdAsync(request.ReleaseId, cancellationToken);
        if (release == null || release.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Release with ID {request.ReleaseId} not found");
        }

        // Get all sprints by IDs in a single query
        var sprints = await _unitOfWork.Repository<Sprint>()
            .Query()
            .Where(s => request.SprintIds.Contains(s.Id))
            .Where(s => s.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        if (sprints.Count != request.SprintIds.Count)
        {
            var foundIds = sprints.Select(s => s.Id).ToList();
            var missingIds = request.SprintIds.Except(foundIds).ToList();
            throw new NotFoundException($"One or more sprints not found: {string.Join(", ", missingIds)}");
        }

        var addedCount = 0;

        // Validate and link each sprint
        foreach (var sprint in sprints)
        {
            // Verify sprint belongs to same project
            if (sprint.ProjectId != release.ProjectId)
            {
                _logger.LogWarning(
                    "Skipping sprint {SprintId} - belongs to different project (ProjectId: {SprintProjectId}, ReleaseProjectId: {ReleaseProjectId})",
                    sprint.Id,
                    sprint.ProjectId,
                    release.ProjectId);
                continue;
            }

            // Validate using domain method
            if (!release.CanAddSprint(sprint))
            {
                _logger.LogWarning(
                    "Skipping sprint {SprintId} - cannot be added to release {ReleaseId} (Status: {ReleaseStatus})",
                    sprint.Id,
                    release.Id,
                    release.Status);
                continue;
            }

            // Check if sprint is already in another release
            if (sprint.ReleaseId.HasValue && sprint.ReleaseId.Value != release.Id)
            {
                _logger.LogWarning(
                    "Skipping sprint {SprintId} - already assigned to release {ExistingReleaseId}",
                    sprint.Id,
                    sprint.ReleaseId.Value);
                continue;
            }

            // Link sprint to release
            sprint.ReleaseId = release.Id;
            addedCount++;
        }

        // Save changes once (batch operation)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Added {AddedCount} sprint(s) to Release {ReleaseId} by user {UserId}",
            addedCount,
            request.ReleaseId,
            _currentUserService.GetUserId());

        return addedCount;
    }
}

