using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, DeleteUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<DeleteUserResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Attempting to delete user {UserId} by user {CurrentUserId}", request.UserId, currentUserId);
        var organizationId = _currentUserService.GetOrganizationId();

        // Check permission
        var hasPermission = await _permissionService.HasPermissionAsync(currentUserId, "users.delete", cancellationToken);
        if (!hasPermission)
        {
            throw new UnauthorizedException("You don't have permission to delete users");
        }

        // Prevent self-deletion
        if (request.UserId == currentUserId)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to delete their own account", currentUserId);
            throw new ValidationException("You cannot delete your own account")
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "UserId", new[] { "You cannot delete your own account" } }
                }
            };
        }

        var userRepo = _unitOfWork.Repository<User>();
        var isSuperAdmin = _currentUserService.IsSuperAdmin();
        
        // SuperAdmin can delete users from any organization, Admin can only delete users from their own organization
        var user = isSuperAdmin
            ? await userRepo.Query()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            : await userRepo.Query()
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.OrganizationId == organizationId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Check if user owns any projects (cannot delete if they do)
        var projectRepo = _unitOfWork.Repository<Project>();
        var ownedProjectsCount = await projectRepo.Query()
            .CountAsync(p => p.OwnerId == request.UserId, cancellationToken);

        if (ownedProjectsCount > 0)
        {
            _logger.LogWarning("Cannot delete user {UserId} because they own {ProjectCount} project(s)", request.UserId, ownedProjectsCount);
            throw new ValidationException($"Cannot delete user because they own {ownedProjectsCount} project(s). Please transfer ownership or delete the projects first.")
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "UserId", new[] { $"User owns {ownedProjectsCount} project(s)" } }
                }
            };
        }

        // Delete related entities before deleting the user
        // 1. Delete RefreshTokens
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        var refreshTokens = await refreshTokenRepo.Query()
            .Where(rt => rt.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var token in refreshTokens)
        {
            refreshTokenRepo.Delete(token);
        }

        // 2. Delete PasswordResetTokens
        var passwordResetTokenRepo = _unitOfWork.Repository<PasswordResetToken>();
        var passwordResetTokens = await passwordResetTokenRepo.Query()
            .Where(prt => prt.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var token in passwordResetTokens)
        {
            passwordResetTokenRepo.Delete(token);
        }

        // 3. Delete ProjectMembers
        var projectMemberRepo = _unitOfWork.Repository<ProjectMember>();
        var projectMembers = await projectMemberRepo.Query()
            .Where(pm => pm.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var member in projectMembers)
        {
            projectMemberRepo.Delete(member);
        }

        // 4. Delete TeamMembers
        var teamMemberRepo = _unitOfWork.Repository<TeamMember>();
        var teamMembers = await teamMemberRepo.Query()
            .Where(tm => tm.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var member in teamMembers)
        {
            teamMemberRepo.Delete(member);
        }

        // 5. Delete UserAIQuota
        var userAIQuotaRepo = _unitOfWork.Repository<UserAIQuota>();
        var userAIQuotas = await userAIQuotaRepo.Query()
            .Where(uq => uq.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var quota in userAIQuotas)
        {
            userAIQuotaRepo.Delete(quota);
        }

        // 6. Delete UserAIQuotaOverride
        var userAIQuotaOverrideRepo = _unitOfWork.Repository<UserAIQuotaOverride>();
        var userAIQuotaOverrides = await userAIQuotaOverrideRepo.Query()
            .Where(uqo => uqo.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var quotaOverride in userAIQuotaOverrides)
        {
            userAIQuotaOverrideRepo.Delete(quotaOverride);
        }

        // 7. Delete UserAIUsageCounter
        var userAIUsageCounterRepo = _unitOfWork.Repository<UserAIUsageCounter>();
        var userAIUsageCounters = await userAIUsageCounterRepo.Query()
            .Where(uuc => uuc.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var counter in userAIUsageCounters)
        {
            userAIUsageCounterRepo.Delete(counter);
        }

        // 8. Delete or nullify other references with Restrict behavior
        // Delete Notifications
        var notificationRepo = _unitOfWork.Repository<Notification>();
        var notifications = await notificationRepo.Query()
            .Where(n => n.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var notification in notifications)
        {
            notificationRepo.Delete(notification);
        }

        // Delete NotificationPreferences
        var notificationPreferenceRepo = _unitOfWork.Repository<NotificationPreference>();
        var notificationPreferences = await notificationPreferenceRepo.Query()
            .Where(np => np.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var preference in notificationPreferences)
        {
            notificationPreferenceRepo.Delete(preference);
        }

        // Delete Mentions
        var mentionRepo = _unitOfWork.Repository<Mention>();
        var mentions = await mentionRepo.Query()
            .Where(m => m.MentionedUserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var mention in mentions)
        {
            mentionRepo.Delete(mention);
        }

        // Delete Invitations created by this user
        var invitationRepo = _unitOfWork.Repository<Invitation>();
        var invitations = await invitationRepo.Query()
            .Where(i => i.CreatedById == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var invitation in invitations)
        {
            invitationRepo.Delete(invitation);
        }

        // Nullify ProjectTask AssigneeId (nullable)
        var projectTaskRepo = _unitOfWork.Repository<ProjectTask>();
        var assignedTasks = await projectTaskRepo.Query()
            .Where(t => t.AssigneeId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var task in assignedTasks)
        {
            task.AssigneeId = null;
            projectTaskRepo.Update(task);
        }

        // Nullify Defect AssignedToId (nullable)
        var defectRepo = _unitOfWork.Repository<Defect>();
        var assignedDefects = await defectRepo.Query()
            .Where(d => d.AssignedToId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var defect in assignedDefects)
        {
            defect.AssignedToId = null;
            defectRepo.Update(defect);
        }

        // Handle ProjectMember InvitedById - set to null if nullable, or delete if required
        var projectMembersInvited = await projectMemberRepo.Query()
            .Where(pm => pm.InvitedById == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var member in projectMembersInvited)
        {
            // Try to set InvitedById to null (if it's nullable)
            // If it's not nullable, we'll need to delete the ProjectMember or handle it differently
            // For now, we'll delete the ProjectMember if InvitedById is required
            projectMemberRepo.Delete(member);
        }

        // Delete AuditLogs (historical but have Restrict constraint)
        var auditLogRepo = _unitOfWork.Repository<AuditLog>();
        var auditLogs = await auditLogRepo.Query()
            .Where(al => al.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var auditLog in auditLogs)
        {
            auditLogRepo.Delete(auditLog);
        }

        // Check for entities with non-nullable CreatedById that would prevent deletion
        var releaseRepo = _unitOfWork.Repository<Release>();
        var releasesCount = await releaseRepo.Query()
            .CountAsync(r => r.CreatedById == request.UserId, cancellationToken);

        var milestoneRepo = _unitOfWork.Repository<Milestone>();
        var milestonesCount = await milestoneRepo.Query()
            .CountAsync(m => m.CreatedById == request.UserId, cancellationToken);

        var taskDependencyRepo = _unitOfWork.Repository<TaskDependency>();
        var taskDependenciesCount = await taskDependencyRepo.Query()
            .CountAsync(td => td.CreatedById == request.UserId, cancellationToken);

        if (releasesCount > 0 || milestonesCount > 0 || taskDependenciesCount > 0)
        {
            var details = new List<string>();
            if (releasesCount > 0) details.Add($"{releasesCount} release(s)");
            if (milestonesCount > 0) details.Add($"{milestonesCount} milestone(s)");
            if (taskDependenciesCount > 0) details.Add($"{taskDependenciesCount} task dependency(ies)");
            
            throw new ValidationException(
                $"Cannot delete user because they have created {string.Join(", ", details)}. " +
                "These are historical records that cannot be deleted. Please contact support for assistance.")
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "UserId", new[] { $"User has created: {string.Join(", ", details)}" } }
                }
            };
        }

        // Nullify nullable UpdatedById references
        var organizationSettingRepo = _unitOfWork.Repository<OrganizationSetting>();
        var orgSettings = await organizationSettingRepo.Query()
            .Where(os => os.UpdatedById == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var setting in orgSettings)
        {
            setting.UpdatedById = null;
            organizationSettingRepo.Update(setting);
        }

        var globalSettingRepo = _unitOfWork.Repository<GlobalSetting>();
        var globalSettings = await globalSettingRepo.Query()
            .Where(gs => gs.UpdatedById == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var setting in globalSettings)
        {
            setting.UpdatedById = null;
            globalSettingRepo.Update(setting);
        }

        // Nullify UserAIQuotaOverride CreatedByUserId (nullable)
        var quotaOverridesCreated = await userAIQuotaOverrideRepo.Query()
            .Where(uqo => uqo.CreatedByUserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var quotaOverride in quotaOverridesCreated)
        {
            quotaOverride.CreatedByUserId = null;
            userAIQuotaOverrideRepo.Update(quotaOverride);
        }

        // Check for AIDecisionLog references (RequestedByUserId is required, ApprovedByUserId/RejectedByUserId are nullable)
        var aiDecisionLogRepo = _unitOfWork.Repository<AIDecisionLog>();
        var aiDecisionLogsRequested = await aiDecisionLogRepo.Query()
            .CountAsync(adl => adl.RequestedByUserId == request.UserId, cancellationToken);

        if (aiDecisionLogsRequested > 0)
        {
            _logger.LogWarning("Cannot delete user {UserId} because they have requested {DecisionCount} AI decision(s)", request.UserId, aiDecisionLogsRequested);
            throw new ValidationException(
                $"Cannot delete user because they have requested {aiDecisionLogsRequested} AI decision(s). " +
                "These are historical records that cannot be deleted. Please contact support for assistance.")
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "UserId", new[] { $"User has requested {aiDecisionLogsRequested} AI decision(s)" } }
                }
            };
        }

        // Nullify nullable AIDecisionLog references
        var aiDecisionLogsApproved = await aiDecisionLogRepo.Query()
            .Where(adl => adl.ApprovedByUserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var decisionLog in aiDecisionLogsApproved)
        {
            decisionLog.ApprovedByUserId = null;
            aiDecisionLogRepo.Update(decisionLog);
        }

        var aiDecisionLogsRejected = await aiDecisionLogRepo.Query()
            .Where(adl => adl.RejectedByUserId == request.UserId)
            .ToListAsync(cancellationToken);
        foreach (var decisionLog in aiDecisionLogsRejected)
        {
            decisionLog.RejectedByUserId = null;
            aiDecisionLogRepo.Update(decisionLog);
        }

        // 9. Finally, delete the User itself
        try
        {
            userRepo.Delete(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("User {UserId} ({Username}) has been permanently deleted by user {CurrentUserId}", 
                request.UserId, user.Username, currentUserId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting user {UserId}. This may be due to remaining foreign key constraints.", request.UserId);
            throw new ValidationException("Cannot delete user due to remaining references in the system. Please contact support.")
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "UserId", new[] { "Database constraint violation. User may have remaining references." } }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting user {UserId}", request.UserId);
            throw;
        }

        return new DeleteUserResponse(true);
    }
}

