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
/// Handler for UpdateReleaseCommand.
/// Updates an existing release with validation.
/// </summary>
public class UpdateReleaseCommandHandler : IRequestHandler<UpdateReleaseCommand, ReleaseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateReleaseCommandHandler> _logger;

    public UpdateReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReleaseDto> Handle(UpdateReleaseCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Get release by ID
        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.CreatedBy)
            .Include(r => r.ReleasedBy)
            .Include(r => r.Sprints)
            .Include(r => r.QualityGates)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.OrganizationId == organizationId, cancellationToken);

        if (release == null)
        {
            throw new NotFoundException($"Release with ID {request.Id} not found");
        }

        // Cannot update deployed releases (except for notes/changelog which are handled separately)
        if (release.Status == ReleaseStatus.Deployed)
        {
            throw new ValidationException("Cannot update a deployed release. Only release notes and changelog can be modified.");
        }

        // Update fields
        release.Name = request.Name;
        release.Version = request.Version;
        release.Description = request.Description;
        release.PlannedDate = request.PlannedDate;
        release.Status = request.Status;
        release.UpdatedAt = DateTimeOffset.UtcNow;

        // Set actual date if deployed
        if (request.Status == ReleaseStatus.Deployed && release.ActualReleaseDate == null)
        {
            release.ActualReleaseDate = DateTimeOffset.UtcNow;
            release.ReleasedById = _currentUserService.GetUserId();
        }

        _unitOfWork.Repository<Release>().Update(release);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Release {ReleaseId} updated by user {UserId}",
            request.Id,
            _currentUserService.GetUserId());

        // Calculate task counts
        var totalTasks = 0;
        var completedTasks = 0;

        foreach (var sprint in release.Sprints)
        {
            foreach (var item in sprint.Items)
            {
                if (item.UserStory?.Tasks != null)
                {
                    var itemTasks = item.UserStory.Tasks;
                    totalTasks += itemTasks.Count;
                    completedTasks += itemTasks.Count(t => t.Status == "DONE" || t.Status == "Done");
                }
            }
        }

        var overallQualityStatus = release.GetOverallQualityStatus();

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
            CompletedTasksCount = completedTasks,
            TotalTasksCount = totalTasks,
            OverallQualityStatus = overallQualityStatus.ToString(),
            CreatedAt = release.CreatedAt,
            CreatedByName = release.CreatedBy?.Username ?? "Unknown",
            ReleasedByName = release.ReleasedBy?.Username
        };
    }
}
