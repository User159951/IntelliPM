using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for GenerateReleaseNotesCommand.
/// Generates release notes for a release based on included sprints and tasks.
/// </summary>
public class GenerateReleaseNotesCommandHandler : IRequestHandler<GenerateReleaseNotesCommand, string>
{
    private readonly IReleaseNotesGenerator _releaseNotesGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateReleaseNotesCommandHandler> _logger;

    public GenerateReleaseNotesCommandHandler(
        IReleaseNotesGenerator releaseNotesGenerator,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<GenerateReleaseNotesCommandHandler> logger)
    {
        _releaseNotesGenerator = releaseNotesGenerator ?? throw new ArgumentNullException(nameof(releaseNotesGenerator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<string> Handle(GenerateReleaseNotesCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Verify release exists and belongs to user's organization
        var release = await _unitOfWork.Repository<Release>().GetByIdAsync(request.ReleaseId, cancellationToken);
        if (release == null || release.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Release with ID {request.ReleaseId} not found");
        }

        // Generate release notes
        var releaseNotes = await _releaseNotesGenerator.GenerateReleaseNotesAsync(request.ReleaseId, cancellationToken);

        _logger.LogInformation(
            "Generated release notes for Release {ReleaseId} by user {UserId}",
            request.ReleaseId,
            _currentUserService.GetUserId());

        return releaseNotes;
    }
}
