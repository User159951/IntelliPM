using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Activity.Queries;

public class GetRecentActivityQueryHandler : IRequestHandler<GetRecentActivityQuery, GetRecentActivityResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRecentActivityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetRecentActivityResponse> Handle(GetRecentActivityQuery request, CancellationToken cancellationToken)
    {
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        var baseQuery = activityRepo.Query()
            .Include(a => a.User)
            .Include(a => a.Project)
            .AsNoTracking();

        // Filter by user's projects if UserId is specified
        if (request.UserId.HasValue && !request.ProjectId.HasValue)
        {
            var projectRepo = _unitOfWork.Repository<Project>();
            var memberRepo = _unitOfWork.Repository<ProjectMember>();
            
            // Get project IDs where user is owner
            var ownedProjectIds = await projectRepo.Query()
                .AsNoTracking()
                .Where(p => p.OwnerId == request.UserId.Value)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
            
            // Get project IDs where user is member
            var memberProjectIds = await memberRepo.Query()
                .AsNoTracking()
                .Where(m => m.UserId == request.UserId.Value)
                .Select(m => m.ProjectId)
                .Distinct()
                .ToListAsync(cancellationToken);
            
            // Combine both lists
            var userProjectIds = ownedProjectIds.Union(memberProjectIds).ToList();
            
            if (userProjectIds.Any())
            {
                baseQuery = baseQuery.Where(a => userProjectIds.Contains(a.ProjectId));
            }
            else
            {
                // User has no projects, return empty result
                return new GetRecentActivityResponse { Activities = new List<ActivityDto>() };
            }
        }

        // Filter by specific project if specified
        if (request.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.ProjectId == request.ProjectId.Value);
        }

        // Order and limit
        var query = baseQuery.OrderByDescending(a => a.CreatedAt);

        // Limit results
        var limit = request.Limit ?? 10;
        var activities = await query
            .Take(limit)
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                Type = a.ActivityType,
                UserId = a.UserId,
                UserName = a.User.FirstName + " " + a.User.LastName,
                UserAvatar = null, // Could add avatar URL to User entity later
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                EntityName = a.EntityName ?? "",
                ProjectId = a.ProjectId,
                ProjectName = a.ProjectName ?? a.Project.Name,
                Timestamp = a.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new GetRecentActivityResponse
        {
            Activities = activities,
        };
    }
}
