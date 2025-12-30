using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for BulkRemoveSprintsFromReleaseCommand.
/// Removes multiple sprints from their releases in a single batch operation.
/// </summary>
public class BulkRemoveSprintsFromReleaseCommandHandler : IRequestHandler<BulkRemoveSprintsFromReleaseCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BulkRemoveSprintsFromReleaseCommandHandler> _logger;

    public BulkRemoveSprintsFromReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<BulkRemoveSprintsFromReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<int> Handle(BulkRemoveSprintsFromReleaseCommand request, CancellationToken cancellationToken)
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

        var removedCount = 0;

        // Remove each sprint from its release
        foreach (var sprint in sprints)
        {
            if (sprint.ReleaseId.HasValue)
            {
                sprint.ReleaseId = null;
                removedCount++;
            }
        }

        // Save changes once (batch operation)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Removed {RemovedCount} sprint(s) from their releases by user {UserId}",
            removedCount,
            _currentUserService.GetUserId());

        return removedCount;
    }
}

