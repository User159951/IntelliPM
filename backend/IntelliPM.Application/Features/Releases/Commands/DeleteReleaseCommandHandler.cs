using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for DeleteReleaseCommand.
/// Deletes a release from the database.
/// </summary>
public class DeleteReleaseCommandHandler : IRequestHandler<DeleteReleaseCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteReleaseCommandHandler> _logger;

    public DeleteReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DeleteReleaseCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Get release by ID with sprints
        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.Sprints)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.OrganizationId == organizationId, cancellationToken);

        if (release == null)
        {
            throw new NotFoundException($"Release with ID {request.Id} not found");
        }

        // Cannot delete deployed releases
        if (release.Status == ReleaseStatus.Deployed)
        {
            throw new ValidationException("Cannot delete a deployed release. Consider archiving instead.");
        }

        // Unlink sprints
        foreach (var sprint in release.Sprints)
        {
            sprint.ReleaseId = null;
            _unitOfWork.Repository<Sprint>().Update(sprint);
        }

        // Delete release
        _unitOfWork.Repository<Release>().Delete(release);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Release {ReleaseId} deleted by user {UserId}",
            request.Id,
            _currentUserService.GetUserId());

        return Unit.Value;
    }
}
