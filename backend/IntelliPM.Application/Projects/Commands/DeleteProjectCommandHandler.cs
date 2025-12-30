using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IntelliPM.Application.Projects.Commands;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProjectCommandHandler> _logger;
    private readonly ICacheService _cache;
    private readonly IMediator _mediator;

    public DeleteProjectCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteProjectCommandHandler> logger, ICacheService cache, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
        _mediator = mediator;
    }

    public async System.Threading.Tasks.Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        // Permission check
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(request.ProjectId, request.CurrentUserId), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanDeleteProject(userRole.Value))
            throw new UnauthorizedException("You don't have permission to delete this project");

        var projectRepo = _unitOfWork.Repository<Project>();
        
        // Load project with navigation properties for cache invalidation
        var project = await projectRepo.Query()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.ProjectId} not found");

        _logger.LogWarning(
            "User {UserId} is permanently deleting project {ProjectId} ({ProjectName}). This action cannot be undone.",
            request.CurrentUserId,
            request.ProjectId,
            project.Name
        );

        // Since all FK relationships use DeleteBehavior.Restrict, we must manually delete related entities
        // Delete in order to respect foreign key constraints

        // 1. Delete ProjectTasks
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var tasks = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var task in tasks)
        {
            taskRepo.Delete(task);
        }

        // 2. Delete SprintItems (must be deleted before Sprints)
        var sprintItemRepo = _unitOfWork.Repository<SprintItem>();
        var sprintItems = await sprintItemRepo.Query()
            .Include(si => si.Sprint)
            .Where(si => si.Sprint != null && si.Sprint.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var sprintItem in sprintItems)
        {
            sprintItemRepo.Delete(sprintItem);
        }

        // 3. Delete KPISnapshots
        var kpiRepo = _unitOfWork.Repository<KPISnapshot>();
        var kpis = await kpiRepo.Query()
            .Include(k => k.Sprint)
            .Where(k => k.Sprint != null && k.Sprint.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var kpi in kpis)
        {
            kpiRepo.Delete(kpi);
        }

        // 4. Delete Sprints
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var sprints = await sprintRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var sprint in sprints)
        {
            sprintRepo.Delete(sprint);
        }

        // 5. Delete ProjectMembers
        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        var members = await memberRepo.Query()
            .Where(m => m.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var member in members)
        {
            memberRepo.Delete(member);
        }

        // 6. Delete Risks
        var riskRepo = _unitOfWork.Repository<Risk>();
        var risks = await riskRepo.Query()
            .Where(r => r.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var risk in risks)
        {
            riskRepo.Delete(risk);
        }

        // 7. Delete Defects
        var defectRepo = _unitOfWork.Repository<Defect>();
        var defects = await defectRepo.Query()
            .Where(d => d.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var defect in defects)
        {
            defectRepo.Delete(defect);
        }

        // 8. Delete Insights
        var insightRepo = _unitOfWork.Repository<Insight>();
        var insights = await insightRepo.Query()
            .Where(i => i.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var insight in insights)
        {
            insightRepo.Delete(insight);
        }

        // 9. Delete Alerts
        var alertRepo = _unitOfWork.Repository<Alert>();
        var alerts = await alertRepo.Query()
            .Where(a => a.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var alert in alerts)
        {
            alertRepo.Delete(alert);
        }

        // 10. Delete Activities
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        var activities = await activityRepo.Query()
            .Where(a => a.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var activity in activities)
        {
            activityRepo.Delete(activity);
        }

        // 11. Delete DocumentStores
        var documentRepo = _unitOfWork.Repository<DocumentStore>();
        var documents = await documentRepo.Query()
            .Where(d => d.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var document in documents)
        {
            documentRepo.Delete(document);
        }

        // 11.5. Delete ProjectTeams
        var projectTeamRepo = _unitOfWork.Repository<ProjectTeam>();
        var projectTeams = await projectTeamRepo.Query()
            .Where(pt => pt.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var projectTeam in projectTeams)
        {
            projectTeamRepo.Delete(projectTeam);
        }

        // 11.8. Delete Notifications
        var notificationRepo = _unitOfWork.Repository<Notification>();
        var notifications = await notificationRepo.Query()
            .Where(n => n.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var notification in notifications)
        {
            notificationRepo.Delete(notification);
        }

        // 12. Delete AIDecisions (must be deleted before AIAgentRuns)
        var decisionRepo = _unitOfWork.Repository<AIDecision>();
        var decisions = await decisionRepo.Query()
            .Where(d => d.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var decision in decisions)
        {
            decisionRepo.Delete(decision);
        }

        // 13. Delete AIAgentRuns
        var agentRunRepo = _unitOfWork.Repository<AIAgentRun>();
        var agentRuns = await agentRunRepo.Query()
            .Where(a => a.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var agentRun in agentRuns)
        {
            agentRunRepo.Delete(agentRun);
        }

        // 14. Delete Tasks (linked to UserStories, must be deleted before UserStories)
        var taskRepo2 = _unitOfWork.Repository<Domain.Entities.Task>();
        var tasks2 = await taskRepo2.Query()
            .Include(t => t.UserStory)
            .Where(t => t.UserStory != null && t.UserStory.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var task in tasks2)
        {
            taskRepo2.Delete(task);
        }

        // 15. Delete BacklogItems (Epics, Features, UserStories)
        var epicRepo = _unitOfWork.Repository<Epic>();
        var epics = await epicRepo.Query()
            .Where(e => e.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var epic in epics)
        {
            epicRepo.Delete(epic);
        }

        var featureRepo = _unitOfWork.Repository<Feature>();
        var features = await featureRepo.Query()
            .Where(f => f.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var feature in features)
        {
            featureRepo.Delete(feature);
        }

        var userStoryRepo = _unitOfWork.Repository<UserStory>();
        var userStories = await userStoryRepo.Query()
            .Where(us => us.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);
        foreach (var userStory in userStories)
        {
            userStoryRepo.Delete(userStory);
        }

        // 16. Finally, delete the Project itself
        projectRepo.Delete(project);

        // Save all changes in a single transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache for project owner and all members (all paginated entries)
        await _cache.RemoveByPrefixAsync($"user-projects:{project.OwnerId}:", cancellationToken);
        await _cache.RemoveByPrefixAsync($"project-details:{project.Id}", cancellationToken);
        await _cache.RemoveByPrefixAsync($"project-tasks:{project.Id}", cancellationToken);
        await _cache.RemoveByPrefixAsync($"dashboard-metrics:", cancellationToken);
        foreach (var member in project.Members)
        {
            await _cache.RemoveByPrefixAsync($"user-projects:{member.UserId}:", cancellationToken);
        }

        _logger.LogInformation(
            "Project {ProjectId} ({ProjectName}) has been permanently deleted by user {UserId}",
            request.ProjectId,
            project.Name,
            request.CurrentUserId
        );
    }
}
