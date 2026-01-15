using MediatR;
using Microsoft.EntityFrameworkCore;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Tasks.Commands;

public class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, AssignTaskResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AssignTaskCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public async Task<AssignTaskResponse> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var task = await taskRepo.GetByIdAsync(request.TaskId, cancellationToken);
        
        if (task == null)
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found");

        // Permission check
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(task.ProjectId, request.UpdatedBy), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanEditTasks(userRole.Value))
            throw new UnauthorizedException("You don't have permission to assign tasks in this project");

        // Verify assignee exists and belongs to the same organization if provided
        if (request.AssigneeId.HasValue)
        {
            var organizationId = _currentUserService.GetOrganizationId();
            var userRepo = _unitOfWork.Repository<User>();
            var assignee = await userRepo.Query()
                .FirstOrDefaultAsync(u => u.Id == request.AssigneeId.Value, cancellationToken);
            
            if (assignee == null)
                throw new ValidationException($"User with ID {request.AssigneeId.Value} not found");
            
            if (assignee.OrganizationId != organizationId)
                throw new ValidationException($"User with ID {request.AssigneeId.Value} does not belong to your organization");
        }

        task.AssigneeId = request.AssigneeId;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        task.UpdatedById = request.UpdatedBy;

        taskRepo.Update(task);

        // Create activity log
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.GetByIdAsync(task.ProjectId, cancellationToken);
        
        var activityType = request.AssigneeId.HasValue ? "task_assigned" : "task_unassigned";
        await activityRepo.AddAsync(new IntelliPM.Domain.Entities.Activity
        {
            UserId = request.UpdatedBy,
            ActivityType = activityType,
            EntityType = "task",
            EntityId = task.Id,
            EntityName = task.Title,
            ProjectId = task.ProjectId,
            ProjectName = project?.Name,
            CreatedAt = DateTimeOffset.UtcNow,
        }, cancellationToken);

        // Create notification when a task is assigned
        if (request.AssigneeId.HasValue)
        {
            var organizationId = _currentUserService.GetOrganizationId();
            var userRepo = _unitOfWork.Repository<User>();
            var assignee = await userRepo.GetByIdAsync(request.AssigneeId.Value, cancellationToken);
            var assigner = await userRepo.GetByIdAsync(request.UpdatedBy, cancellationToken);
            
            var notificationRepo = _unitOfWork.Repository<Notification>();
            await notificationRepo.AddAsync(new Notification
            {
                UserId = request.AssigneeId.Value,
                OrganizationId = organizationId,
                Type = "task_assigned",
                Message = $"{assigner?.FirstName} {assigner?.LastName} assigned you to '{task.Title}'",
                EntityType = "task",
                EntityId = task.Id,
                ProjectId = task.ProjectId,
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssignTaskResponse(
            task.Id,
            task.AssigneeId,
            task.UpdatedAt
        );
    }
}

