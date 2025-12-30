using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for UpdateReleaseNotesCommand.
/// Updates release notes either by auto-generation or manual input.
/// </summary>
public class UpdateReleaseNotesCommandHandler : IRequestHandler<UpdateReleaseNotesCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReleaseNotesGenerator _releaseNotesGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateReleaseNotesCommandHandler> _logger;

    public UpdateReleaseNotesCommandHandler(
        IUnitOfWork unitOfWork,
        IReleaseNotesGenerator releaseNotesGenerator,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<UpdateReleaseNotesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _releaseNotesGenerator = releaseNotesGenerator ?? throw new ArgumentNullException(nameof(releaseNotesGenerator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<Unit> Handle(UpdateReleaseNotesCommand request, CancellationToken cancellationToken)
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

        if (request.AutoGenerate)
        {
            // Auto-generate release notes
            var generatedNotes = await _releaseNotesGenerator.GenerateReleaseNotesAsync(request.ReleaseId, cancellationToken);

            // Truncate if exceeds max length
            if (generatedNotes.Length > ReleaseConstants.MaxReleaseNotesLength)
            {
                generatedNotes = generatedNotes.Substring(0, ReleaseConstants.MaxReleaseNotesLength);
                _logger.LogWarning(
                    "Generated release notes for Release {ReleaseId} exceeded max length and were truncated",
                    request.ReleaseId);
            }

            release.ReleaseNotes = generatedNotes;
            release.UpdatedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation(
                "Auto-generated release notes for Release {ReleaseId} by user {UserId}",
                request.ReleaseId,
                _currentUserService.GetUserId());

            // Publish domain event
            var releaseNotesGeneratedEvent = new ReleaseNotesGeneratedEvent
            {
                ReleaseId = release.Id,
                ProjectId = release.ProjectId,
                GeneratedAt = DateTimeOffset.UtcNow,
                GeneratedBy = _currentUserService.GetUserId()
            };

            await _mediator.Publish(releaseNotesGeneratedEvent, cancellationToken);
        }
        else
        {
            // Use manually provided notes
            if (request.ReleaseNotes != null && request.ReleaseNotes.Length > ReleaseConstants.MaxReleaseNotesLength)
            {
                throw new ValidationException($"Release notes cannot exceed {ReleaseConstants.MaxReleaseNotesLength} characters");
            }

            release.ReleaseNotes = request.ReleaseNotes;
            release.UpdatedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation(
                "Manually updated release notes for Release {ReleaseId} by user {UserId}",
                request.ReleaseId,
                _currentUserService.GetUserId());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
