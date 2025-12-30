using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Projects.Commands;

public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, ArchiveProjectResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMediator _mediator;

    public ArchiveProjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task<ArchiveProjectResponse> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        // Permission check
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(request.ProjectId, request.CurrentUserId), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanDeleteProject(userRole.Value))
            throw new UnauthorizedException("You don't have permission to archive this project");

        var projectRepo = _unitOfWork.Repository<Project>();
        
        // Load existing project with members for cache invalidation
        var project = await projectRepo.Query()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.ProjectId} not found");

        // Check if already archived
        if (project.Status == "Archived")
            throw new InvalidOperationException($"Project {request.ProjectId} is already archived");

        // Set Status to Archived (soft delete)
        project.Status = "Archived";
        project.UpdatedAt = DateTimeOffset.UtcNow;

        // Save changes
        projectRepo.Update(project);

        // Create activity log
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        await activityRepo.AddAsync(new IntelliPM.Domain.Entities.Activity
        {
            UserId = request.CurrentUserId,
            ActivityType = "project_archived",
            EntityType = "project",
            EntityId = project.Id,
            EntityName = project.Name,
            ProjectId = project.Id,
            ProjectName = project.Name,
            CreatedAt = DateTimeOffset.UtcNow,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache for project owner and all members (all paginated entries)
        await _cache.RemoveByPrefixAsync($"user-projects:{project.OwnerId}:", cancellationToken);
        await _cache.RemoveByPrefixAsync($"project-details:{project.Id}", cancellationToken);
        await _cache.RemoveByPrefixAsync($"project-tasks:{project.Id}", cancellationToken);
        foreach (var member in project.Members)
        {
            await _cache.RemoveByPrefixAsync($"user-projects:{member.UserId}:", cancellationToken);
        }

        return new ArchiveProjectResponse(
            project.Id,
            project.Name,
            project.Status,
            project.UpdatedAt
        );
    }
}

