using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for DeployReleaseCommand.
/// Deploys a release after validating quality gates.
/// </summary>
public class DeployReleaseCommandHandler : IRequestHandler<DeployReleaseCommand, ReleaseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeployReleaseCommandHandler> _logger;

    public DeployReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeployReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReleaseDto> Handle(DeployReleaseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (userId == 0 || organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.CreatedBy)
            .Include(r => r.QualityGates)
            .Include(r => r.Sprints)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.OrganizationId == organizationId, cancellationToken);

        if (release == null)
        {
            throw new NotFoundException($"Release with ID {request.Id} not found");
        }

        // Check if release can be deployed
        if (!release.CanDeploy())
        {
            throw new InvalidOperationException($"Release must be in ReadyForDeployment status to be deployed. Current status: {release.Status}");
        }

        // Validate quality gates
        if (!release.AreQualityGatesPassed())
        {
            var failedGates = release.QualityGates
                .Where(qg => qg.IsRequired && qg.Status != Domain.Enums.QualityGateStatus.Passed && qg.Status != Domain.Enums.QualityGateStatus.Skipped)
                .ToList();

            var gateNames = string.Join(", ", failedGates.Select(qg => qg.Type.ToString()));
            throw new ValidationException($"Cannot deploy: {failedGates.Count} required quality gate(s) not passed: {gateNames}");
        }

        // Deploy release using domain method
        release.MarkAsDeployed(userId);

        _unitOfWork.Repository<Release>().Update(release);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Release {ReleaseId} deployed successfully by user {UserId}",
            request.Id,
            userId);

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
