using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for UpdateChangeLogCommand.
/// Updates changelog either by auto-generation or manual input.
/// </summary>
public class UpdateChangeLogCommandHandler : IRequestHandler<UpdateChangeLogCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReleaseNotesGenerator _releaseNotesGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateChangeLogCommandHandler> _logger;

    public UpdateChangeLogCommandHandler(
        IUnitOfWork unitOfWork,
        IReleaseNotesGenerator releaseNotesGenerator,
        ICurrentUserService currentUserService,
        ILogger<UpdateChangeLogCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _releaseNotesGenerator = releaseNotesGenerator ?? throw new ArgumentNullException(nameof(releaseNotesGenerator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<Unit> Handle(UpdateChangeLogCommand request, CancellationToken cancellationToken)
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
            // Auto-generate changelog
            var generatedChangelog = await _releaseNotesGenerator.GenerateChangeLogAsync(request.ReleaseId, cancellationToken);

            // Truncate if exceeds max length
            if (generatedChangelog.Length > ReleaseConstants.MaxChangeLogLength)
            {
                generatedChangelog = generatedChangelog.Substring(0, ReleaseConstants.MaxChangeLogLength);
                _logger.LogWarning(
                    "Generated changelog for Release {ReleaseId} exceeded max length and were truncated",
                    request.ReleaseId);
            }

            release.ChangeLog = generatedChangelog;
            release.UpdatedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation(
                "Auto-generated changelog for Release {ReleaseId} by user {UserId}",
                request.ReleaseId,
                _currentUserService.GetUserId());
        }
        else
        {
            // Use manually provided changelog
            if (request.ChangeLog != null && request.ChangeLog.Length > ReleaseConstants.MaxChangeLogLength)
            {
                throw new ValidationException($"Change log cannot exceed {ReleaseConstants.MaxChangeLogLength} characters");
            }

            release.ChangeLog = request.ChangeLog;
            release.UpdatedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation(
                "Manually updated changelog for Release {ReleaseId} by user {UserId}",
                request.ReleaseId,
                _currentUserService.GetUserId());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
