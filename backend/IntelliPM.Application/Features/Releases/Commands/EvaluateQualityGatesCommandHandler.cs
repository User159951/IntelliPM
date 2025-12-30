using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Commands;

/// <summary>
/// Handler for EvaluateQualityGatesCommand.
/// Evaluates all quality gates for a release and updates the database.
/// </summary>
public class EvaluateQualityGatesCommandHandler : IRequestHandler<EvaluateQualityGatesCommand, List<QualityGateDto>>
{
    private readonly IQualityGateChecker _qualityGateChecker;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EvaluateQualityGatesCommandHandler> _logger;

    public EvaluateQualityGatesCommandHandler(
        IQualityGateChecker qualityGateChecker,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<EvaluateQualityGatesCommandHandler> logger)
    {
        _qualityGateChecker = qualityGateChecker ?? throw new ArgumentNullException(nameof(qualityGateChecker));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<List<QualityGateDto>> Handle(EvaluateQualityGatesCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Get release and verify ownership
        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.QualityGates)
                .ThenInclude(qg => qg.CheckedByUser)
            .FirstOrDefaultAsync(r => r.Id == request.ReleaseId, cancellationToken);

        if (release == null || release.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Release with ID {request.ReleaseId} not found");
        }

        // Evaluate all quality gates
        var results = await _qualityGateChecker.EvaluateAllQualityGatesAsync(request.ReleaseId, cancellationToken);

        var qualityGateRepo = _unitOfWork.Repository<QualityGate>();
        var qualityGateDtos = new List<QualityGateDto>();

        // Update or create quality gates based on results
        foreach (var result in results)
        {
            var existingGate = release.QualityGates.FirstOrDefault(qg => qg.Type == result.GateType);

            if (existingGate != null)
            {
                // Update existing gate
                existingGate.Status = result.Status;
                existingGate.Message = result.Message;
                existingGate.Details = result.Details;
                existingGate.CheckedAt = result.CheckedAt;
                existingGate.Threshold = result.Threshold;
                existingGate.ActualValue = result.ActualValue;
                // For system checks, don't update CheckedByUserId (keep existing or null)
                // Only manual approvals set CheckedByUserId
            }
            else
            {
                // Create new gate
                var newGate = new QualityGate
                {
                    ReleaseId = release.Id,
                    Type = result.GateType,
                    Status = result.Status,
                    Message = result.Message,
                    Details = result.Details,
                    CheckedAt = result.CheckedAt,
                    Threshold = result.Threshold,
                    ActualValue = result.ActualValue,
                    IsRequired = result.GateType != QualityGateType.ManualApproval, // Manual approval can be optional
                    CreatedAt = DateTimeOffset.UtcNow
                };
                // CheckedByUserId is only set for manual approvals via ApproveQualityGateCommand

                await qualityGateRepo.AddAsync(newGate, cancellationToken);
                release.QualityGates.Add(newGate);
                existingGate = newGate;
            }

            // Map to DTO
            qualityGateDtos.Add(new QualityGateDto
            {
                Id = existingGate.Id,
                ReleaseId = existingGate.ReleaseId,
                Type = existingGate.Type.ToString(),
                Status = existingGate.Status.ToString(),
                IsRequired = existingGate.IsRequired,
                Threshold = existingGate.Threshold,
                ActualValue = existingGate.ActualValue,
                Message = existingGate.Message,
                Details = existingGate.Details,
                CheckedAt = existingGate.CheckedAt,
                CheckedByName = existingGate.CheckedByUser?.Username
            });
        }

        release.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Evaluated {GateCount} quality gates for Release {ReleaseId} by user {UserId}",
            results.Count,
            request.ReleaseId,
            _currentUserService.GetUserId());

        return qualityGateDtos;
    }
}

