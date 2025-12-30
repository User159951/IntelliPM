using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Handler for UpdateMilestoneCommand.
/// Updates an existing milestone with validation.
/// </summary>
public class UpdateMilestoneCommandHandler : IRequestHandler<UpdateMilestoneCommand, MilestoneDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMilestoneValidator _milestoneValidator;
    private readonly ILogger<UpdateMilestoneCommandHandler> _logger;

    public UpdateMilestoneCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMilestoneValidator milestoneValidator,
        ILogger<UpdateMilestoneCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _milestoneValidator = milestoneValidator ?? throw new ArgumentNullException(nameof(milestoneValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<MilestoneDto> Handle(UpdateMilestoneCommand request, CancellationToken cancellationToken)
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

        // Validate due date if changed
        if (milestone.DueDate != request.DueDate)
        {
            var dueDateValidation = await _milestoneValidator.ValidateDueDateAsync(
                milestone.ProjectId, request.DueDate, cancellationToken);
            if (!dueDateValidation.IsValid)
            {
                throw new ValidationException(string.Join(", ", dueDateValidation.Errors));
            }
        }

        // Update properties
        milestone.Name = request.Name;
        milestone.Description = request.Description;
        milestone.DueDate = request.DueDate;
        milestone.UpdatedAt = DateTimeOffset.UtcNow;

        // Update progress using domain method (includes validation and status updates)
        try
        {
            milestone.UpdateProgress(request.Progress);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new ValidationException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationException(ex.Message);
        }

        _unitOfWork.Repository<Milestone>().Update(milestone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Milestone {MilestoneId} updated by user {UserId}",
            milestone.Id,
            _currentUserService.GetUserId());

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

