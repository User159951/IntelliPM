using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for ApproveQualityGateCommand.
/// Manually approves a quality gate (typically ManualApproval type).
/// EXCLUSIVE permission: Only Tester/QA role can approve quality gates.
/// </summary>
public class ApproveQualityGateCommandHandler : IRequestHandler<ApproveQualityGateCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<ApproveQualityGateCommandHandler> _logger;

    public ApproveQualityGateCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<ApproveQualityGateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<Unit> Handle(ApproveQualityGateCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (userId == 0 || organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Get release and verify ownership
        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.QualityGates)
            .FirstOrDefaultAsync(r => r.Id == request.ReleaseId, cancellationToken);

        if (release == null || release.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Release with ID {request.ReleaseId} not found");
        }

        // Permission check - EXCLUSIVE to Tester/QA
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(release.ProjectId, userId), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanValidateQualityGate(userRole.Value))
            throw new UnauthorizedException($"Only Tester/QA can validate quality gates. Your role: {userRole.Value}");

        var qualityGateRepo = _unitOfWork.Repository<QualityGate>();

        // Find existing quality gate by type
        var qualityGate = release.QualityGates.FirstOrDefault(qg => qg.Type == request.GateType);

        if (qualityGate == null)
        {
            // Create new quality gate
            qualityGate = new QualityGate
            {
                ReleaseId = release.Id,
                Type = request.GateType,
                Status = QualityGateStatus.Passed,
                IsRequired = request.GateType == QualityGateType.ManualApproval,
                Message = $"Manually approved by user {userId}",
                CheckedAt = DateTimeOffset.UtcNow,
                CheckedByUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await qualityGateRepo.AddAsync(qualityGate, cancellationToken);
            release.QualityGates.Add(qualityGate);
        }
        else
        {
            // Update existing gate
            qualityGate.Status = QualityGateStatus.Passed;
            qualityGate.CheckedByUserId = userId;
            qualityGate.CheckedAt = DateTimeOffset.UtcNow;
            qualityGate.Message = $"Manually approved by user {userId}";
        }

        release.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Approved quality gate {GateType} for Release {ReleaseId} by user {UserId} (Role: {Role})",
            request.GateType,
            request.ReleaseId,
            userId,
            userRole.Value);

        return Unit.Value;
    }
}

