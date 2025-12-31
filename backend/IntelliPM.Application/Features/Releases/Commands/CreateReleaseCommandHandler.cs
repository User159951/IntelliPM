using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for CreateReleaseCommand.
/// Creates a new release for a project with validation.
/// </summary>
public class CreateReleaseCommandHandler : IRequestHandler<CreateReleaseCommand, ReleaseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateReleaseCommandHandler> _logger;

    public CreateReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<CreateReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReleaseDto> Handle(CreateReleaseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (userId == 0 || organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Verify project exists and belongs to user's organization
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");
        }

        // Create release
        var release = new Release
        {
            ProjectId = request.ProjectId,
            OrganizationId = organizationId,
            Name = request.Name,
            Version = request.Version,
            Description = request.Description,
            PlannedDate = request.PlannedDate,
            Status = ReleaseStatus.Planned,
            Type = request.Type,
            IsPreRelease = request.IsPreRelease,
            TagName = request.TagName,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedById = userId
        };

        await _unitOfWork.Repository<Release>().AddAsync(release, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Release {ReleaseId} created for project {ProjectId} by user {UserId}",
            release.Id,
            request.ProjectId,
            userId);

        // Add sprints if provided
        if (request.SprintIds.Any())
        {
            var bulkAddCommand = new BulkAddSprintsToReleaseCommand
            {
                ReleaseId = release.Id,
                SprintIds = request.SprintIds
            };
            await _mediator.Send(bulkAddCommand, cancellationToken);
        }

        // Reload with related data for DTO mapping
        release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.CreatedBy)
            .Include(r => r.Sprints)
            .Include(r => r.QualityGates)
            .FirstAsync(r => r.Id == release.Id, cancellationToken);

        // Map to DTO
        return new ReleaseDto
        {
            Id = release.Id,
            ProjectId = release.ProjectId,
            Name = release.Name,
            Version = release.Version,
            Description = release.Description,
            Type = release.Type.ToString(),
            Status = release.Status.ToString(),
            PlannedDate = release.PlannedDate,
            ActualReleaseDate = release.ActualReleaseDate,
            ReleaseNotes = release.ReleaseNotes,
            ChangeLog = release.ChangeLog,
            IsPreRelease = release.IsPreRelease,
            TagName = release.TagName,
            SprintCount = release.Sprints.Count,
            CompletedTasksCount = 0,
            TotalTasksCount = 0,
            OverallQualityStatus = release.GetOverallQualityStatus().ToString(),
            CreatedAt = release.CreatedAt,
            CreatedByName = release.CreatedBy?.Username ?? "Unknown"
        };
    }
}
