using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for DeployReleaseCommand.
/// Deploys a release after validating quality gates and QA approval.
/// Requires QA approval before deployment can proceed.
/// </summary>
public class DeployReleaseCommandHandler : IRequestHandler<DeployReleaseCommand, ReleaseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<DeployReleaseCommandHandler> _logger;

    public DeployReleaseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<DeployReleaseCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
                .ThenInclude(qg => qg.CheckedByUser)
            .Include(r => r.Sprints)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.OrganizationId == organizationId, cancellationToken);

        if (release == null)
        {
            throw new NotFoundException($"Release with ID {request.Id} not found");
        }

        // Permission check - ProductOwner or ScrumMaster can deploy
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(release.ProjectId, userId), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanManageSprints(userRole.Value)) // ProductOwner or ScrumMaster
            throw new UnauthorizedException($"Only ProductOwner or ScrumMaster can deploy releases. Your role: {userRole.Value}");

        // Check if release can be deployed
        if (!release.CanDeploy())
        {
            throw new InvalidOperationException($"Release must be in ReadyForDeployment status to be deployed. Current status: {release.Status}");
        }

        // Validate blocking quality gates first (most restrictive)
        var blockingGates = release.QualityGates
            .Where(qg => qg.IsBlocking && qg.Status != QualityGateStatus.Passed && qg.Status != QualityGateStatus.Skipped)
            .ToList();

        if (blockingGates.Any())
        {
            var gateNames = string.Join(", ", blockingGates.Select(qg => qg.Type.ToString()));
            throw new QualityGateNotPassedException(
                $"Cannot deploy: {blockingGates.Count} blocking quality gate(s) not passed: {gateNames}",
                blockingGates.Select(qg => qg.Type.ToString()));
        }

        // Validate required quality gates (for backward compatibility)
        if (!release.AreQualityGatesPassed())
        {
            var failedGates = release.QualityGates
                .Where(qg => qg.IsRequired && qg.Status != QualityGateStatus.Passed && qg.Status != QualityGateStatus.Skipped)
                .ToList();

            var gateNames = string.Join(", ", failedGates.Select(qg => qg.Type.ToString()));
            throw new QualityGateNotPassedException(
                $"Cannot deploy: {failedGates.Count} required quality gate(s) not passed: {gateNames}",
                failedGates.Select(qg => qg.Type.ToString()));
        }

        // QA Approval Check: Verify that at least one quality gate has been validated by a QA/Tester
        var qaValidatedGates = release.QualityGates
            .Where(qg => qg.CheckedByUserId.HasValue && qg.Status == QualityGateStatus.Passed)
            .ToList();

        if (qaValidatedGates.Any())
        {
            // Check if the user who validated is a Tester/QA
            var validatedByUserIds = qaValidatedGates.Select(qg => qg.CheckedByUserId!.Value).Distinct().ToList();
            var memberRepo = _unitOfWork.Repository<ProjectMember>();
            
            var qaValidators = await memberRepo.Query()
                .Where(m => m.ProjectId == release.ProjectId && 
                           validatedByUserIds.Contains(m.UserId) && 
                           m.Role == ProjectRole.Tester)
                .Select(m => m.UserId)
                .ToListAsync(cancellationToken);

            if (!qaValidators.Any())
            {
                throw new ValidationException("Release cannot be deployed: Quality gates must be validated by a Tester/QA before deployment. No QA approval found.");
            }
        }
        else
        {
            // If no gates have been manually validated, check if there are any required manual approval gates
            var manualApprovalGates = release.QualityGates
                .Where(qg => qg.Type == QualityGateType.ManualApproval && qg.IsRequired)
                .ToList();

            if (manualApprovalGates.Any())
            {
                var unapprovedGates = manualApprovalGates
                    .Where(qg => qg.Status != QualityGateStatus.Passed)
                    .ToList();

                if (unapprovedGates.Any())
                {
                    throw new ValidationException($"Release cannot be deployed: {unapprovedGates.Count} required manual approval quality gate(s) must be approved by QA before deployment.");
                }
            }
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
