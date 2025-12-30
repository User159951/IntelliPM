using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Handler for CreateMilestoneCommand.
/// Creates a new milestone for a project with validation.
/// </summary>
public class CreateMilestoneCommandHandler : IRequestHandler<CreateMilestoneCommand, MilestoneDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMilestoneValidator _milestoneValidator;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateMilestoneCommandHandler> _logger;

    public CreateMilestoneCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMilestoneValidator milestoneValidator,
        IMediator mediator,
        ILogger<CreateMilestoneCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _milestoneValidator = milestoneValidator ?? throw new ArgumentNullException(nameof(milestoneValidator));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<MilestoneDto> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (userId == 0 || organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Verify project exists and belongs to user's organization
        var project = await _unitOfWork.Repository<Project>()
            .GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null || project.OrganizationId != organizationId)
        {
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");
        }

        // Validate due date
        var dueDateValidation = await _milestoneValidator.ValidateDueDateAsync(
            request.ProjectId, request.DueDate, cancellationToken);
        if (!dueDateValidation.IsValid)
        {
            throw new ValidationException(string.Join(", ", dueDateValidation.Errors));
        }

        // Parse Type enum
        if (!Enum.TryParse<MilestoneType>(request.Type, out var milestoneType))
        {
            throw new ValidationException($"Invalid milestone type: {request.Type}");
        }

        // Create milestone
        var milestone = new Milestone
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            Description = request.Description,
            Type = milestoneType,
            Status = MilestoneStatus.Pending,
            DueDate = request.DueDate,
            Progress = request.Progress,
            OrganizationId = organizationId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedById = userId
        };

        await _unitOfWork.Repository<Milestone>().AddAsync(milestone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Milestone {MilestoneId} created for project {ProjectId} by user {UserId}",
            milestone.Id,
            request.ProjectId,
            userId);

        // Publish domain event
        var createdEvent = new MilestoneCreatedEvent
        {
            MilestoneId = milestone.Id,
            ProjectId = milestone.ProjectId,
            Name = milestone.Name,
            DueDate = milestone.DueDate,
            Type = (int)milestone.Type
        };
        await _mediator.Publish(createdEvent, cancellationToken);

        // Reload with CreatedBy for DTO mapping
        milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.CreatedBy)
            .FirstAsync(m => m.Id == milestone.Id, cancellationToken);

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

