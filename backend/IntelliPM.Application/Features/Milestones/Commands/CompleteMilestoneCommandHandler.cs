using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Handler for CompleteMilestoneCommand.
/// Marks a milestone as completed with validation.
/// </summary>
public class CompleteMilestoneCommandHandler : IRequestHandler<CompleteMilestoneCommand, MilestoneDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMilestoneValidator _milestoneValidator;
    private readonly IMediator _mediator;
    private readonly ILogger<CompleteMilestoneCommandHandler> _logger;

    public CompleteMilestoneCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMilestoneValidator milestoneValidator,
        IMediator mediator,
        ILogger<CompleteMilestoneCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _milestoneValidator = milestoneValidator ?? throw new ArgumentNullException(nameof(milestoneValidator));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<MilestoneDto> Handle(CompleteMilestoneCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Get milestone by ID
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.CreatedBy)
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.OrganizationId == organizationId, cancellationToken);

        if (milestone == null)
        {
            throw new NotFoundException($"Milestone with ID {request.Id} not found");
        }

        // Validate completion
        var completedAt = request.CompletedAt ?? DateTimeOffset.UtcNow;
        var validation = _milestoneValidator.ValidateCompletion(milestone, completedAt);
        if (!validation.IsValid)
        {
            throw new ValidationException(string.Join(", ", validation.Errors));
        }

        // Mark as completed using domain method
        try
        {
            milestone.MarkAsCompleted(completedAt);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationException(ex.Message);
        }

        _unitOfWork.Repository<Milestone>().Update(milestone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Milestone {MilestoneId} marked as completed by user {UserId}",
            milestone.Id,
            _currentUserService.GetUserId());

        // Publish domain event
        var completedEvent = new MilestoneCompletedEvent
        {
            MilestoneId = milestone.Id,
            ProjectId = milestone.ProjectId,
            CompletedAt = completedAt
        };
        await _mediator.Publish(completedEvent, cancellationToken);

        return MapToDto(milestone);
    }

    private static MilestoneDto MapToDto(Milestone milestone)
    {
        var now = DateTimeOffset.UtcNow;
        var daysUntilDue = (milestone.DueDate - now).Days;
        var isOverdue = milestone.DueDate < now 
            && milestone.Status != MilestoneStatus.Completed 
            && milestone.Status != MilestoneStatus.Cancelled;

        return new MilestoneDto
        {
            Id = milestone.Id,
            ProjectId = milestone.ProjectId,
            Name = milestone.Name,
            Description = milestone.Description ?? string.Empty,
            Type = milestone.Type.ToString(),
            Status = milestone.Status.ToString(),
            DueDate = milestone.DueDate,
            CompletedAt = milestone.CompletedAt,
            Progress = milestone.Progress,
            DaysUntilDue = daysUntilDue,
            IsOverdue = isOverdue,
            CreatedAt = milestone.CreatedAt,
            CreatedByName = milestone.CreatedBy?.Username ?? "Unknown"
        };
    }
}

