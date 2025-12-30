using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Projects.Commands;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<CreateProjectResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // Check project creation policy
        var settingsRepo = _unitOfWork.Repository<GlobalSetting>();
        var allowedRolesSetting = await settingsRepo.Query()
            .FirstOrDefaultAsync(s => s.Key == "ProjectCreation.AllowedRoles", cancellationToken);

        if (allowedRolesSetting != null)
        {
            // Get the owner's GlobalRole
            var ownerUserRepo = _unitOfWork.Repository<User>();
            var ownerUser = await ownerUserRepo.Query()
                .IgnoreQueryFilters() // Bypass multi-tenancy filter for permission check
                .FirstOrDefaultAsync(u => u.Id == request.OwnerId, cancellationToken);

            if (ownerUser == null)
            {
                throw new NotFoundException($"User with ID {request.OwnerId} not found");
            }

            var allowedRoles = allowedRolesSetting.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(r => r.Trim())
                .ToList();

            // If setting is "Admin", only Admin users can create projects
            if (allowedRoles.Count == 1 && allowedRoles[0].Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (ownerUser.GlobalRole != GlobalRole.Admin)
                {
                    throw new UnauthorizedException("Only administrators can create projects");
                }
            }
            // If setting is "Admin,User" or "All", all users can create
            else
            {
                var ownerRoleString = ownerUser.GlobalRole.ToString();
                var isAllowed = allowedRoles.Any(r => r.Equals(ownerRoleString, StringComparison.OrdinalIgnoreCase) 
                                                   || r.Equals("All", StringComparison.OrdinalIgnoreCase));
                if (!isAllowed)
                {
                    throw new UnauthorizedException($"Your role ({ownerUser.GlobalRole}) is not allowed to create projects");
                }
            }
        }

        // Validate status
        var validStatuses = new[] { "Active", "Planned", "OnHold" };
        if (!validStatuses.Contains(request.Status))
        {
            throw new ArgumentException($"Status must be one of: {string.Join(", ", validStatuses)}");
        }

        // Validate start date if provided
        if (request.StartDate.HasValue && request.StartDate.Value < DateTimeOffset.UtcNow.Date)
        {
            throw new ArgumentException("Start date must be today or in the future");
        }

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            SprintDurationDays = request.SprintDurationDays,
            OwnerId = request.OwnerId,
            Status = request.Status,
            CreatedAt = request.StartDate ?? DateTimeOffset.UtcNow
        };

        var repo = _unitOfWork.Repository<Project>();
        await repo.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // Save to get project ID

        // Add project members (owner is automatically included)
        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        var memberIds = new HashSet<int> { request.OwnerId }; // Owner is always included
        
        if (request.MemberIds != null && request.MemberIds.Any())
        {
            foreach (var memberId in request.MemberIds)
            {
                if (memberId != request.OwnerId) // Don't add owner twice
                {
                    memberIds.Add(memberId);
                }
            }
        }

        // Verify all member IDs exist
        var userRepo = _unitOfWork.Repository<User>();
        var existingUserIds = await userRepo.Query()
            .Where(u => memberIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var notificationRepo = _unitOfWork.Repository<Notification>();
        var ownerRepo = _unitOfWork.Repository<User>();
        var owner = await ownerRepo.GetByIdAsync(request.OwnerId, cancellationToken);
        
        foreach (var memberId in memberIds)
        {
            if (existingUserIds.Contains(memberId))
            {
                await memberRepo.AddAsync(new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = memberId,
                    Role = memberId == request.OwnerId ? Domain.Enums.ProjectRole.ProductOwner : Domain.Enums.ProjectRole.Developer,
                    InvitedById = request.OwnerId,
                    InvitedAt = DateTime.UtcNow,
                    JoinedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
                
                // Create notification for project invite (except for owner)
                if (memberId != request.OwnerId)
                {
                    await notificationRepo.AddAsync(new Notification
                    {
                        UserId = memberId,
                        Type = "project_invite",
                        Message = $"{owner?.FirstName} {owner?.LastName} added you to project '{project.Name}'",
                        EntityType = "project",
                        EntityId = project.Id,
                        ProjectId = project.Id,
                        IsRead = false,
                        CreatedAt = DateTimeOffset.UtcNow
                    }, cancellationToken);
                }
            }
        }

        // Create activity log
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        await activityRepo.AddAsync(new IntelliPM.Domain.Entities.Activity
        {
            UserId = request.OwnerId,
            ActivityType = "project_created",
            EntityType = "project",
            EntityId = project.Id,
            EntityName = project.Name,
            ProjectId = project.Id,
            ProjectName = project.Name,
            CreatedAt = DateTimeOffset.UtcNow,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event via Outbox pattern
        var projectCreatedEvent = new ProjectCreatedEvent
        {
            ProjectId = project.Id,
            OrganizationId = project.OrganizationId,
            ProjectName = project.Name,
            ProjectType = project.Type,
            OwnerId = project.OwnerId,
            Status = project.Status
        };

        var eventType = typeof(ProjectCreatedEvent).AssemblyQualifiedName ?? typeof(ProjectCreatedEvent).FullName ?? "ProjectCreatedEvent";
        var eventPayload = JsonSerializer.Serialize(projectCreatedEvent);
        var idempotencyKey = $"project-created-{project.Id}-{DateTimeOffset.UtcNow.Ticks}";
        var outboxMessage = OutboxMessage.Create(eventType, eventPayload, idempotencyKey);

        var outboxRepo = _unitOfWork.Repository<OutboxMessage>();
        await outboxRepo.AddAsync(outboxMessage, cancellationToken);

        // Publish member added events for each member
        foreach (var memberId in memberIds)
        {
            var memberAddedEvent = new MemberAddedToProjectEvent
            {
                ProjectId = project.Id,
                OrganizationId = project.OrganizationId,
                UserId = memberId,
                Role = memberId == request.OwnerId ? "ProductOwner" : "Developer"
            };

            var memberEventType = typeof(MemberAddedToProjectEvent).AssemblyQualifiedName ?? typeof(MemberAddedToProjectEvent).FullName ?? "MemberAddedToProjectEvent";
            var memberEventPayload = JsonSerializer.Serialize(memberAddedEvent);
            var memberIdempotencyKey = $"member-added-{project.Id}-{memberId}-{DateTimeOffset.UtcNow.Ticks}";
            var memberOutboxMessage = OutboxMessage.Create(memberEventType, memberEventPayload, memberIdempotencyKey);
            await outboxRepo.AddAsync(memberOutboxMessage, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate user's projects cache (all paginated entries)
        await _cache.RemoveByPrefixAsync($"user-projects:{request.OwnerId}:", cancellationToken);
        // Also invalidate for any members added
        if (request.MemberIds != null)
        {
            foreach (var memberId in request.MemberIds)
            {
                await _cache.RemoveByPrefixAsync($"user-projects:{memberId}:", cancellationToken);
            }
        }

        return new CreateProjectResponse(project.Id, project.Name, project.Description, project.Type);
    }
}

